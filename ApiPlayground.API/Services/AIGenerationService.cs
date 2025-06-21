using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using System.Text.Json;
using System.Text;

namespace ApiPlayground.API.Services
{
    public class AIGenerationService : IAIGenerationService
    {
        private readonly HttpClient _httpClient;
        private readonly IIntegrationService _integrationService;
        private readonly ILogger<AIGenerationService> _logger;
        private readonly string _apiKey;

        public AIGenerationService(
            HttpClient httpClient,
            IConfiguration configuration,
            IIntegrationService integrationService,
            ILogger<AIGenerationService> logger)
        {
            _httpClient = httpClient;
            _integrationService = integrationService;
            _logger = logger;
            _apiKey = configuration["OPENAI_API_KEY"] ?? 
                     Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? 
                     throw new InvalidOperationException("OpenAI API key is required. Please set OPENAI_API_KEY environment variable.");
            
            _httpClient.BaseAddress = new Uri("https://api.openai.com/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<AIGenerationResponse> GenerateIntegrationFromDescriptionAsync(AIGenerationRequest request)
        {
            try
            {
                var systemPrompt = """
                You are an API integration expert. Generate a complete API integration based on the user's description.
                
                Return ONLY a valid JSON object with this structure:
                {
                  "integration": {
                    "name": "Integration Name",
                    "description": "Description",
                    "executionMode": "Sequential",
                    "authentication": null,
                    "requests": [
                      {
                        "name": "Request Name",
                        "method": "GET",
                        "url": "https://api.example.com/endpoint",
                        "headers": {"Content-Type": "application/json"},
                        "body": null,
                        "order": 1,
                        "canRunInParallel": true,
                        "dependsOn": [],
                        "retryConfig": {
                          "maxAttempts": 3,
                          "delayMs": 1000,
                          "exponentialBackoff": true,
                          "retryOnStatusCodes": [500, 502, 503, 504]
                        },
                        "conditionalRules": []
                      }
                    ]
                  },
                  "explanation": "Explanation of the generated integration",
                  "suggestions": ["Suggestion 1", "Suggestion 2"]
                }
                
                Use these HTTP methods: GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS
                Include placeholders like {{userId}}, {{apiKey}} where appropriate.
                """;

                var userPrompt = $"""
                Generate an API integration for: {request.Description}
                
                {(string.IsNullOrEmpty(request.BaseUrl) ? "" : $"Base URL: {request.BaseUrl}")}
                {(request.Examples.Any() ? $"Examples: {string.Join(", ", request.Examples)}" : "")}
                {(string.IsNullOrEmpty(request.AuthenticationType) ? "" : $"Authentication: {request.AuthenticationType}")}
                """;

                var chatRequest = new
                {
                    model = "gpt-4o",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.7,
                    max_tokens = 2000
                };

                var jsonContent = JsonSerializer.Serialize(chatRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("v1/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var openAiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                if (openAiResponse?.Choices?.Any() == true)
                {
                    var assistantMessage = openAiResponse.Choices[0].Message.Content;
                    _logger.LogInformation("OpenAI Response: {Content}", assistantMessage);

                    try
                    {
                        var jsonResponse = JsonSerializer.Deserialize<AIGenerationResponse>(assistantMessage, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (jsonResponse?.Integration != null)
                        {
                            // Set IDs for the integration and requests
                            jsonResponse.Integration.Id = Guid.NewGuid().ToString();
                            jsonResponse.Integration.CreatedAt = DateTime.UtcNow;
                            jsonResponse.Integration.UpdatedAt = DateTime.UtcNow;
                            
                            foreach (var req in jsonResponse.Integration.Requests)
                            {
                                req.Id = Guid.NewGuid().ToString();
                                req.IntegrationId = jsonResponse.Integration.Id;
                                req.CreatedAt = DateTime.UtcNow;
                                req.UpdatedAt = DateTime.UtcNow;
                            }

                            // Save the integration
                            await _integrationService.CreateIntegrationAsync(jsonResponse.Integration);
                            _logger.LogInformation("Successfully saved AI-generated integration: {IntegrationId}", jsonResponse.Integration.Id);
                            
                            return jsonResponse;
                        }
                        else
                        {
                            _logger.LogWarning("AI response parsing successful but Integration is null");
                            return await CreateFallbackResponseAsync(request.Description, "AI generated empty integration response.");
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to parse AI response JSON: {Content}", assistantMessage);
                        return await CreateFallbackResponseAsync(request.Description, "Failed to parse AI response format.");
                    }
                }

                return await CreateFallbackResponseAsync(request.Description, "No response from OpenAI");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating integration from description: {Description}", request.Description);
                return await CreateFallbackResponseAsync(request.Description, $"Error occurred during AI generation: {ex.Message}");
            }
        }

        public async Task<List<string>> SuggestImprovementsAsync(string integrationId)
        {
            try
            {
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                if (integration == null)
                    return new List<string> { "Integration not found" };

                var systemPrompt = """
                You are an API integration expert. Analyze the provided integration and suggest improvements.
                
                Return ONLY a JSON array of strings with specific, actionable suggestions.
                Example: ["Add error handling for 429 rate limits", "Use exponential backoff for retries"]
                """;

                var userPrompt = $"""
                Analyze this integration and suggest improvements:
                
                Name: {integration.Name}
                Description: {integration.Description}
                Execution Mode: {integration.ExecutionMode}
                Requests Count: {integration.Requests.Count}
                
                Requests:
                {string.Join("\n", integration.Requests.Select(r => $"- {r.Name}: {r.Method} {r.Url}"))}
                """;

                var chatRequest = new
                {
                    model = "gpt-4o",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.8,
                    max_tokens = 1000
                };

                var jsonContent = JsonSerializer.Serialize(chatRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("v1/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var openAiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                if (openAiResponse?.Choices?.Any() == true)
                {
                    var assistantMessage = openAiResponse.Choices[0].Message.Content;
                    var suggestions = JsonSerializer.Deserialize<List<string>>(assistantMessage) ?? new List<string>();
                    return suggestions;
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suggesting improvements for integration: {IntegrationId}", integrationId);
                return new List<string> { "Error generating suggestions. Please try again." };
            }
        }

        public async Task<string> ExplainIntegrationAsync(string integrationId)
        {
            try
            {
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                if (integration == null)
                    return "Integration not found";

                var systemPrompt = """
                You are an API integration expert. Explain how the provided integration works in clear, non-technical language.
                
                Return ONLY a plain text explanation without JSON formatting.
                """;

                var userPrompt = $"""
                Explain this integration:
                
                Name: {integration.Name}
                Description: {integration.Description}
                Execution Mode: {integration.ExecutionMode}
                
                Requests:
                {string.Join("\n", integration.Requests.Select((r, i) => $"{i + 1}. {r.Name}: {r.Method} {r.Url}"))}
                """;

                var chatRequest = new
                {
                    model = "gpt-4o",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.6,
                    max_tokens = 1500
                };

                var jsonContent = JsonSerializer.Serialize(chatRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("v1/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var openAiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                if (openAiResponse?.Choices?.Any() == true)
                {
                    return openAiResponse.Choices[0].Message.Content;
                }

                return "No explanation available";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error explaining integration: {IntegrationId}", integrationId);
                return "Error generating explanation. Please try again.";
            }
        }

        private AIGenerationResponse CreateFallbackResponse(string description, string errorMessage)
        {
            var fallbackIntegration = new Integration 
            { 
                Id = Guid.NewGuid().ToString(),
                Name = "Fallback Integration", 
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Requests = CreateFallbackRequests(description)
            };

            // Set integration ID for all requests
            foreach (var req in fallbackIntegration.Requests)
            {
                req.IntegrationId = fallbackIntegration.Id;
            }

            // Save the fallback integration to the database
            try
            {
                _integrationService.CreateIntegrationAsync(fallbackIntegration).Wait();
                _logger.LogInformation("Saved fallback integration with ID: {IntegrationId}", fallbackIntegration.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save fallback integration");
            }

            return new AIGenerationResponse
            {
                Integration = fallbackIntegration,
                Explanation = errorMessage,
                Suggestions = new List<string> 
                { 
                    "Check your OpenAI API key",
                    "Try with a simpler description",
                    "Manually create the integration"
                }
            };
        }

        private async Task<AIGenerationResponse> CreateFallbackResponseAsync(string description, string errorMessage)
        {
            var fallbackIntegration = new Integration 
            { 
                Id = Guid.NewGuid().ToString(),
                Name = "Fallback Integration", 
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Requests = CreateFallbackRequests(description)
            };

            // Set integration ID for all requests
            foreach (var req in fallbackIntegration.Requests)
            {
                req.IntegrationId = fallbackIntegration.Id;
            }

            // Save the fallback integration to the database
            try
            {
                await _integrationService.CreateIntegrationAsync(fallbackIntegration);
                _logger.LogInformation("Saved fallback integration with ID: {IntegrationId}", fallbackIntegration.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save fallback integration");
            }

            return new AIGenerationResponse
            {
                Integration = fallbackIntegration,
                Explanation = errorMessage,
                Suggestions = new List<string> 
                { 
                    "Check your OpenAI API key",
                    "Try with a simpler description",
                    "Manually create the integration"
                }
            };
        }

        private List<Request> CreateFallbackRequests(string description)
        {
            var baseUrl = "https://api.example.com";
            var requests = new List<Request>();

            // Create sample requests based on common patterns
            if (description.ToLower().Contains("weather"))
            {
                requests.AddRange(new[]
                {
                    CreateSampleRequest("Get Current Weather", HttpMethodType.GET, "https://api.openweathermap.org/data/2.5/weather?q={{city}}&appid={{apiKey}}", 1),
                    CreateSampleRequest("Get Weather Forecast", HttpMethodType.GET, "https://api.openweathermap.org/data/2.5/forecast?q={{city}}&appid={{apiKey}}", 2)
                });
            }
            else if (description.ToLower().Contains("github") || description.ToLower().Contains("user"))
            {
                requests.AddRange(new[]
                {
                    CreateSampleRequest("Get User Profile", HttpMethodType.GET, "https://api.github.com/users/{{username}}", 1),
                    CreateSampleRequest("Get User Repositories", HttpMethodType.GET, "https://api.github.com/users/{{username}}/repos", 2)
                });
            }
            else if (description.ToLower().Contains("post") || description.ToLower().Contains("create"))
            {
                requests.AddRange(new[]
                {
                    CreateSampleRequest("Create Resource", HttpMethodType.POST, $"{baseUrl}/{{resource}}", 1),
                    CreateSampleRequest("Get Created Resource", HttpMethodType.GET, $"{baseUrl}/{{resource}}/{{id}}", 2)
                });
            }
            else
            {
                // Generic fallback requests
                requests.AddRange(new[]
                {
                    CreateSampleRequest("Get Data", HttpMethodType.GET, $"{baseUrl}/data?query={{query}}", 1),
                    CreateSampleRequest("Create Entry", HttpMethodType.POST, $"{baseUrl}/entries", 2),
                    CreateSampleRequest("Update Entry", HttpMethodType.PUT, $"{baseUrl}/entries/{{id}}", 3)
                });
            }

            return requests;
        }

        private Request CreateSampleRequest(string name, HttpMethodType method, string url, int order)
        {
            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            };

            if (method == HttpMethodType.POST || method == HttpMethodType.PUT || method == HttpMethodType.PATCH)
            {
                headers.Add("Authorization", "Bearer {{token}}");
            }

            return new Request
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Method = method,
                Url = url,
                Headers = headers,
                Body = method == HttpMethodType.POST || method == HttpMethodType.PUT ? "{\"data\": \"{{value}}\"}" : null,
                Order = order,
                CanRunInParallel = order > 1,
                DependsOn = new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RetryConfig = new RetryConfig
                {
                    MaxAttempts = 3,
                    DelayMs = 1000,
                    ExponentialBackoff = true,
                    RetryOnStatusCodes = new List<int> { 500, 502, 503, 504 }
                },
                ConditionalRules = new List<ConditionalRule>()
            };
        }

        // OpenAI API response models
        private class OpenAIResponse
        {
            public List<Choice>? Choices { get; set; }
        }

        private class Choice
        {
            public Message Message { get; set; } = new();
        }

        private class Message
        {
            public string Content { get; set; } = string.Empty;
        }
    }
} 