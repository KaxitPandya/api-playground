using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
using ApiPlayground.API.Data;
using ApiPlayground.API.Extensions;
using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace ApiPlayground.API.Services;

public class ExecutionService : IExecutionService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExecutionService> _logger;
    private readonly IOAuthService _oauthService;

    public ExecutionService(
        ApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<ExecutionService> logger,
        IOAuthService oauthService)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _oauthService = oauthService;
    }

    public async Task<List<RequestResult>> ExecuteIntegrationAsync(string integrationId, Dictionary<string, string> placeholders)
    {
        var config = new ExecutionConfig
        {
            Placeholders = placeholders,
            Mode = ExecutionMode.Sequential,
            EnableRetries = true
        };
        
        return await ExecuteIntegrationWithConfigAsync(integrationId, config);
    }

    public async Task<List<RequestResult>> ExecuteIntegrationWithConfigAsync(string integrationId, ExecutionConfig config)
    {
        _logger.LogInformation("Executing integration: {IntegrationId} with mode: {Mode}", integrationId, config.Mode);
        
        var integration = await _context.Integrations
            .Include(i => i.Requests.OrderBy(r => r.Order))
            .FirstOrDefaultAsync(i => i.Id == integrationId);
            
        if (integration == null)
        {
            _logger.LogError("Integration not found: {IntegrationId}", integrationId);
            throw new ArgumentException($"Integration with ID {integrationId} not found");
        }

        // Apply authentication if configured
        await ApplyAuthenticationAsync(integration, config.Placeholders);

        var results = config.Mode switch
        {
            ExecutionMode.Parallel => await ExecuteRequestsInParallelAsync(integration.Requests, config.Placeholders, config.MaxParallelRequests),
            ExecutionMode.Conditional => await ExecuteRequestsWithConditionalFlowAsync(integration.Requests, config.Placeholders),
            _ => await ExecuteRequestsSequentiallyAsync(integration.Requests, config.Placeholders, config.EnableRetries)
        };
        
        _logger.LogInformation(
            "Integration execution completed: {IntegrationName}, Requests: {RequestCount}",
            integration.Name,
            results.Count);
            
        return results;
    }

    public async Task<RequestResult> ExecuteRequestAsync(string requestId, Dictionary<string, string> placeholders)
    {
        var request = await _context.Requests.FirstOrDefaultAsync(r => r.Id == requestId);
        
        if (request == null)
        {
            throw new ArgumentException($"Request with ID {requestId} not found");
        }
        
        return await ExecuteRequestWithRetriesAsync(request, placeholders);
    }

    public async Task<RequestResult> ExecuteRequestWithRetriesAsync(Request request, Dictionary<string, string> placeholders, RetryConfig? retryConfig = null)
    {
        var config = retryConfig ?? request.RetryConfig ?? new RetryConfig();
        var attempt = 1;
        
        while (attempt <= config.MaxAttempts)
        {
            var result = await ExecuteSingleRequestAsync(request, placeholders, new List<RequestResult>(), attempt);
            
            // Check if we should retry
            if (attempt < config.MaxAttempts && ShouldRetry(result, config))
            {
                _logger.LogWarning("Request {RequestName} failed with status {StatusCode}, retrying (attempt {Attempt}/{MaxAttempts})", 
                    request.Name, result.StatusCode, attempt, config.MaxAttempts);
                
                var delay = config.ExponentialBackoff 
                    ? config.DelayMs * (int)Math.Pow(2, attempt - 1)
                    : config.DelayMs;
                    
                await Task.Delay(delay);
                attempt++;
                continue;
            }
            
            return result;
        }
        
        // This shouldn't be reached, but just in case
        return await ExecuteSingleRequestAsync(request, placeholders, new List<RequestResult>(), attempt);
    }

    public async Task<List<RequestResult>> ExecuteRequestsInParallelAsync(List<Request> requests, Dictionary<string, string> placeholders, int maxParallelRequests = 5)
    {
        _logger.LogInformation("Executing {RequestCount} requests in parallel with max concurrency: {MaxParallel}", requests.Count, maxParallelRequests);
        
        var results = new List<RequestResult>();
        var semaphore = new SemaphoreSlim(maxParallelRequests, maxParallelRequests);
        
        // Group requests by dependencies
        var independentRequests = requests.Where(r => !r.DependsOn.Any() && r.CanRunInParallel).ToList();
        var dependentRequests = requests.Where(r => r.DependsOn.Any() || !r.CanRunInParallel).ToList();
        
        // Execute independent requests in parallel
        var parallelTasks = independentRequests.Select(async request =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await ExecuteRequestWithRetriesAsync(request, placeholders);
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        var parallelResults = await Task.WhenAll(parallelTasks);
        results.AddRange(parallelResults);
        
        // Execute dependent requests sequentially
        foreach (var request in dependentRequests.OrderBy(r => r.Order))
        {
            var result = await ExecuteRequestWithRetriesAsync(request, placeholders);
            results.Add(result);
        }
        
        return results;
    }

    public async Task<List<RequestResult>> ExecuteRequestsWithConditionalFlowAsync(List<Request> requests, Dictionary<string, string> placeholders)
    {
        _logger.LogInformation("Executing {RequestCount} requests with conditional flow", requests.Count);
        
        var results = new List<RequestResult>();
        var executedRequestIds = new HashSet<string>();
        
        foreach (var request in requests.OrderBy(r => r.Order))
        {
            // Check if this request should be executed based on conditions
            if (!ShouldExecuteRequest(request, results))
            {
                _logger.LogInformation("Skipping request {RequestName} due to conditional rules", request.Name);
                continue;
            }
            
            var result = await ExecuteRequestWithRetriesAsync(request, placeholders);
            results.Add(result);
            executedRequestIds.Add(request.Id);
            
            // Process conditional rules
            var action = ProcessConditionalRules(request, result);
            
            if (action == "stop")
            {
                _logger.LogInformation("Stopping execution due to conditional rule in request: {RequestName}", request.Name);
                break;
            }
        }
        
        return results;
    }

    private async Task<List<RequestResult>> ExecuteRequestsSequentiallyAsync(List<Request> requests, Dictionary<string, string> placeholders, bool enableRetries)
    {
        var results = new List<RequestResult>();
        
        foreach (var request in requests.OrderBy(r => r.Order))
        {
            var result = enableRetries 
                ? await ExecuteRequestWithRetriesAsync(request, placeholders)
                : await ExecuteSingleRequestAsync(request, placeholders, results);
                
            results.Add(result);
        }
        
        return results;
    }

    private async Task<RequestResult> ExecuteSingleRequestAsync(Request request, Dictionary<string, string> placeholders, ICollection<RequestResult> previousResults, int attemptNumber = 1)
    {
        _logger.LogInformation("Executing request: {RequestName} (attempt {AttemptNumber})", request.Name, attemptNumber);
        
        var stopwatch = new Stopwatch();
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMilliseconds(30000); // 30 second timeout
        
        // Process URL and replace placeholders
        var url = ProcessPlaceholders(request.Url, placeholders, previousResults);
        
        // Create HttpRequestMessage
        var httpMethod = ConvertToSystemHttpMethod(request.Method);
        var httpRequest = new HttpRequestMessage(httpMethod, url);
        
        // Add headers
        foreach (var header in request.Headers)
        {
            var headerValue = ProcessPlaceholders(header.Value, placeholders, previousResults);
            httpRequest.Headers.TryAddWithoutValidation(header.Key, headerValue);
        }
        
        // Add body if needed
        if (!string.IsNullOrEmpty(request.Body) && NeedsBody(request.Method))
        {
            var processedBody = ProcessPlaceholders(request.Body, placeholders, previousResults);
            httpRequest.Content = new StringContent(processedBody, Encoding.UTF8, "application/json");
        }
        
        // Execute the request and measure time
        stopwatch.Start();
        HttpResponseMessage response;
        string? errorMessage = null;
        
        try
        {
            response = await client.SendAsync(httpRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing request: {RequestName}", request.Name);
            errorMessage = ex.Message;
            
            // Create error result
            var errorResult = new RequestResult
            {
                Id = Guid.NewGuid().ToString(),
                RequestId = request.Id,
                RequestName = request.Name,
                StatusCode = 0,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Response = $"{{\"error\": \"{ex.Message}\"}}",
                Error = ex.Message,
                AttemptNumber = attemptNumber,
                IsRetry = attemptNumber > 1,
                ExecutedAt = DateTime.UtcNow,
                Request = request
            };
            
            await _context.RequestResults.AddAsync(errorResult);
            await _context.SaveChangesAsync();
            
            return errorResult;
        }
        
        stopwatch.Stop();
        
        // Read response
        var responseContent = await response.Content.ReadAsStringAsync();
        
        // Create result
        var result = new RequestResult
        {
            Id = Guid.NewGuid().ToString(),
            RequestId = request.Id,
            RequestName = request.Name,
            StatusCode = (int)response.StatusCode,
            ResponseTimeMs = stopwatch.ElapsedMilliseconds,
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
            Response = responseContent,
            Error = errorMessage,
            AttemptNumber = attemptNumber,
            IsRetry = attemptNumber > 1,
            ExecutedAt = DateTime.UtcNow,
            Request = request
        };
        
        await _context.RequestResults.AddAsync(result);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation(
            "Request completed: {RequestName}, Status: {StatusCode}, Time: {ResponseTimeMs}ms, Attempt: {AttemptNumber}",
            request.Name, 
            result.StatusCode, 
            result.ResponseTimeMs,
            attemptNumber);
            
        return result;
    }

    private async Task ApplyAuthenticationAsync(Integration integration, Dictionary<string, string> placeholders)
    {
        if (integration.Authentication == null)
            return;

        switch (integration.Authentication.Type)
        {
            case AuthenticationType.OAuth2:
                var token = await _oauthService.GetValidAccessTokenAsync(integration.Id);
                if (!string.IsNullOrEmpty(token))
                {
                    placeholders["token"] = token;
                }
                break;
                
            case AuthenticationType.BearerToken:
                if (!string.IsNullOrEmpty(integration.Authentication.BearerToken))
                {
                    placeholders["token"] = integration.Authentication.BearerToken;
                }
                break;
                
            case AuthenticationType.ApiKey:
                if (integration.Authentication.ApiKey != null)
                {
                    placeholders[integration.Authentication.ApiKey.Key] = integration.Authentication.ApiKey.Value;
                }
                break;
                
            case AuthenticationType.BasicAuth:
                if (integration.Authentication.BasicAuth != null)
                {
                    var credentials = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{integration.Authentication.BasicAuth.Username}:{integration.Authentication.BasicAuth.Password}"));
                    placeholders["basicAuth"] = $"Basic {credentials}";
                }
                break;
        }
    }

    private bool ShouldRetry(RequestResult result, RetryConfig config)
    {
        if (result.StatusCode == 0) // Network error
            return true;
            
        return config.RetryOnStatusCodes.Contains(result.StatusCode);
    }

    private bool ShouldExecuteRequest(Request request, List<RequestResult> previousResults)
    {
        if (!request.ConditionalRules.Any())
            return true;
            
        foreach (var rule in request.ConditionalRules)
        {
            if (!EvaluateCondition(rule, previousResults))
            {
                return false;
            }
        }
        
        return true;
    }

    private bool EvaluateCondition(ConditionalRule rule, List<RequestResult> previousResults)
    {
        try
        {
            // Find the relevant result based on the condition
            var relevantResult = previousResults.LastOrDefault();
            if (relevantResult?.Response == null)
                return false;

            var actualValue = relevantResult.Response.ExtractValueByJsonPath(rule.Condition)?.ToString();
            
            return rule.Operator.ToLower() switch
            {
                "equals" => actualValue == rule.ExpectedValue,
                "contains" => actualValue?.Contains(rule.ExpectedValue) == true,
                "greater_than" => double.TryParse(actualValue, out var actual) && double.TryParse(rule.ExpectedValue, out var expected) && actual > expected,
                "less_than" => double.TryParse(actualValue, out var actual2) && double.TryParse(rule.ExpectedValue, out var expected2) && actual2 < expected2,
                "not_equals" => actualValue != rule.ExpectedValue,
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating condition: {Condition}", rule.Condition);
            return false;
        }
    }

    private string ProcessConditionalRules(Request request, RequestResult result)
    {
        foreach (var rule in request.ConditionalRules)
        {
            if (EvaluateCondition(rule, new List<RequestResult> { result }))
            {
                return rule.Action;
            }
        }
        
        return "continue";
    }

    private string ProcessPlaceholders(
        string input, 
        Dictionary<string, string> placeholders, 
        ICollection<RequestResult> previousResults)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }
        
        // Replace manual placeholders
        foreach (var placeholder in placeholders)
        {
            input = input.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
        }
        
        // Check for JSONPath expressions from previous responses
        if (previousResults.Any())
        {
            var regex = new Regex(@"\{\{(\$\..*?)\}\}");
            var matches = regex.Matches(input);
            
            foreach (Match match in matches)
            {
                var expression = match.Groups[1].Value;
                var requestIndex = 0;
                var jsonPath = expression;
                
                // Extract request index if specified as {{$[0].name}}
                if (expression.StartsWith("$[") && expression.Contains("]."))
                {
                    var indexMatch = Regex.Match(expression, @"\$\[(\d+)\]\.(.*)");
                    if (indexMatch.Success)
                    {
                        requestIndex = int.Parse(indexMatch.Groups[1].Value);
                        jsonPath = "$." + indexMatch.Groups[2].Value;
                    }
                }
                
                if (previousResults.Count > requestIndex)
                {
                    var result = previousResults.ElementAt(requestIndex);
                    if (!string.IsNullOrEmpty(result.Response))
                    {
                        try
                        {
                            // Use our custom JSON extension method for better JSONPath support
                            var extractedValue = result.Response.ExtractValueByJsonPath(jsonPath);
                            
                            if (extractedValue != null)
                            {
                                input = input.Replace($"{{{{{expression}}}}}", extractedValue.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing JSONPath: {JSONPath}", jsonPath);
                            
                            // Fallback to Newtonsoft.Json approach
                            try
                            {
                                var json = JObject.Parse(result.Response);
                                var token = json.SelectToken(jsonPath);
                                
                                if (token != null)
                                {
                                    input = input.Replace($"{{{{{expression}}}}}", token.ToString());
                                }
                            }
                            catch (Exception fallbackEx)
                            {
                                _logger.LogError(fallbackEx, "Fallback JSONPath extraction also failed: {JSONPath}", jsonPath);
                            }
                        }
                    }
                }
            }
        }
        
        return input;
    }
    
    private bool NeedsBody(HttpMethodType method)
    {
        return method == HttpMethodType.POST || 
               method == HttpMethodType.PUT || 
               method == HttpMethodType.PATCH;
    }
    
    private System.Net.Http.HttpMethod ConvertToSystemHttpMethod(HttpMethodType method)
    {
        return method switch
        {
            HttpMethodType.GET => System.Net.Http.HttpMethod.Get,
            HttpMethodType.POST => System.Net.Http.HttpMethod.Post,
            HttpMethodType.PUT => System.Net.Http.HttpMethod.Put,
            HttpMethodType.DELETE => System.Net.Http.HttpMethod.Delete,
            HttpMethodType.PATCH => System.Net.Http.HttpMethod.Patch,
            HttpMethodType.HEAD => System.Net.Http.HttpMethod.Head,
            HttpMethodType.OPTIONS => System.Net.Http.HttpMethod.Options,
            _ => System.Net.Http.HttpMethod.Get
        };
    }
}
