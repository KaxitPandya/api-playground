using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Text;
using OpenApiDocument = Microsoft.OpenApi.Models.OpenApiDocument;
using OpenApiParameter = Microsoft.OpenApi.Models.OpenApiParameter;

namespace ApiPlayground.API.Services
{
    public class OpenAPIImportService : IOpenAPIImportService
    {
        private readonly IIntegrationService _integrationService;
        private readonly ILogger<OpenAPIImportService> _logger;
        private readonly HttpClient _httpClient;

        public OpenAPIImportService(
            IIntegrationService integrationService,
            ILogger<OpenAPIImportService> logger,
            HttpClient httpClient)
        {
            _integrationService = integrationService;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<OpenAPIImportResponse> ImportFromUrlAsync(string url, string? baseUrl = null, List<string>? selectedOperations = null)
        {
            try
            {
                _logger.LogInformation("Importing OpenAPI from URL: {Url}", url);
                
                var content = await _httpClient.GetStringAsync(url);
                return await ImportFromFileContentAsync(content, baseUrl, selectedOperations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing OpenAPI from URL: {Url}", url);
                return new OpenAPIImportResponse
                {
                    Integration = new Integration { Name = "Failed Import", Description = $"Failed to import from {url}" },
                    Warnings = new List<string> { $"Error: {ex.Message}" }
                };
            }
        }

        public async Task<OpenAPIImportResponse> ImportFromFileContentAsync(string fileContent, string? baseUrl = null, List<string>? selectedOperations = null)
        {
            try
            {
                _logger.LogInformation("Importing OpenAPI from file content");
                
                var integration = await ConvertOpenAPIToIntegrationAsync(fileContent, baseUrl, selectedOperations);
                
                // Save the integration
                await _integrationService.CreateIntegrationAsync(integration);
                
                var availableOperations = await GetAvailableOperationsAsync(fileContent, false);
                var importedCount = integration.Requests.Count;
                
                return new OpenAPIImportResponse
                {
                    Integration = integration,
                    AvailableOperations = availableOperations,
                    ImportedCount = importedCount,
                    Warnings = new List<string>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing OpenAPI from file content");
                return new OpenAPIImportResponse
                {
                    Integration = new Integration { Name = "Failed Import", Description = "Failed to import from file content" },
                    Warnings = new List<string> { $"Error: {ex.Message}" }
                };
            }
        }

        public async Task<List<string>> GetAvailableOperationsAsync(string urlOrContent, bool isUrl = true)
        {
            try
            {
                string content;
                if (isUrl)
                {
                    content = await _httpClient.GetStringAsync(urlOrContent);
                }
                else
                {
                    content = urlOrContent;
                }

                var reader = new OpenApiStringReader();
            var document = reader.Read(content, out var diagnostic);
                var operations = new List<string>();

                foreach (var path in document.Paths)
                {
                    foreach (var operation in path.Value.Operations)
                    {
                        if (operation.Value != null)
                        {
                            var operationId = operation.Value.OperationId ?? $"{path.Key}_{operation.Key}";
                            var summary = operation.Value.Summary ?? operationId;
                            operations.Add($"{operationId}: {summary}");
                        }
                    }
                }

                return operations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available operations");
                return new List<string> { $"Error: {ex.Message}" };
            }
        }

        public async Task<Integration> ConvertOpenAPIToIntegrationAsync(string openApiContent, string? baseUrl = null, List<string>? selectedOperations = null)
        {
            try
            {
                var reader = new OpenApiStringReader();
            var document = reader.Read(openApiContent, out var diagnostic);
                
                var integration = new Integration
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = document.Info?.Title ?? "Imported Integration",
                    Description = document.Info?.Description ?? "Imported from OpenAPI specification",
                    ExecutionMode = ExecutionMode.Sequential,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Determine base URL
                var serverUrl = baseUrl;
                if (string.IsNullOrEmpty(serverUrl) && document.Servers?.Any() == true)
                {
                    serverUrl = document.Servers.First().Url;
                }

                var requests = new List<Request>();
                var order = 1;

                foreach (var path in document.Paths)
                {
                    foreach (var operation in path.Value.Operations)
                    {
                        if (operation.Value != null)
                        {
                            var operationId = operation.Value.OperationId ?? $"{path.Key}_{operation.Key}";
                            
                            _logger.LogInformation("Processing operation: {OperationId}", operationId);
                            
                            // Check if this operation should be included
                            // selectedOperations contains items in format "operationId: Summary", so we need to match properly
                            if (selectedOperations != null && selectedOperations.Any())
                            {
                                var shouldInclude = selectedOperations.Any(selected => 
                                    selected.StartsWith($"{operationId}:", StringComparison.OrdinalIgnoreCase) || 
                                    selected.Equals(operationId, StringComparison.OrdinalIgnoreCase));
                                
                                _logger.LogInformation("Operation {OperationId} should be included: {ShouldInclude}", operationId, shouldInclude);
                                
                                if (!shouldInclude)
                                    continue;
                            }

                            var request = new Request
                            {
                                Id = Guid.NewGuid().ToString(),
                                IntegrationId = integration.Id,
                                Name = operation.Value.Summary ?? operationId,
                                Method = MapHttpMethod(operation.Key),
                                Url = CombineUrls(serverUrl, path.Key),
                                Headers = new Dictionary<string, string>(),
                                Order = order++,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                CanRunInParallel = true,
                                DependsOn = new List<string>(),
                                RetryConfig = new RetryConfig
                                {
                                    MaxAttempts = 3,
                                    DelayMs = 1000,
                                    ExponentialBackoff = true,
                                    RetryOnStatusCodes = new List<int> { 500, 502, 503, 504 }
                                }
                            };

                            // Add content type header if operation has request body
                            if (operation.Value.RequestBody != null)
                            {
                                var contentType = operation.Value.RequestBody.Content?.Keys.FirstOrDefault() ?? "application/json";
                                request.Headers["Content-Type"] = contentType;
                                
                                // Add sample body if available
                                if (operation.Value.RequestBody.Content?.Values.FirstOrDefault()?.Example != null)
                                {
                                    request.Body = operation.Value.RequestBody.Content.Values.First().Example.ToString();
                                }
                            }

                            // Add authorization header placeholder if security is defined
                            if (operation.Value.Security?.Any() == true || document.SecurityRequirements?.Any() == true)
                            {
                                request.Headers["Authorization"] = "Bearer {{token}}";
                            }

                            // Add query parameters as placeholders
                            foreach (var parameter in operation.Value.Parameters ?? new List<OpenApiParameter>())
                            {
                                if (parameter.In == ParameterLocation.Query)
                                {
                                    var placeholder = $"{{{{{parameter.Name}}}}}";
                                    request.Url += request.Url.Contains('?') ? $"&{parameter.Name}={placeholder}" : $"?{parameter.Name}={placeholder}";
                                }
                                else if (parameter.In == ParameterLocation.Path)
                                {
                                    // Path parameters are already in the URL, just ensure they're in placeholder format
                                    request.Url = request.Url.Replace($"{{{parameter.Name}}}", $"{{{{{parameter.Name}}}}}");
                                }
                                else if (parameter.In == ParameterLocation.Header)
                                {
                                    request.Headers[parameter.Name] = $"{{{{{parameter.Name}}}}}";
                                }
                            }

                            requests.Add(request);
                        }
                    }
                }

                integration.Requests = requests;
                return integration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting OpenAPI to integration");
                throw new InvalidOperationException($"Failed to convert OpenAPI specification: {ex.Message}", ex);
            }
        }

        private static HttpMethodType MapHttpMethod(OperationType operationType)
        {
            return operationType switch
            {
                OperationType.Get => HttpMethodType.GET,
                OperationType.Post => HttpMethodType.POST,
                OperationType.Put => HttpMethodType.PUT,
                OperationType.Delete => HttpMethodType.DELETE,
                OperationType.Patch => HttpMethodType.PATCH,
                OperationType.Head => HttpMethodType.HEAD,
                OperationType.Options => HttpMethodType.OPTIONS,
                _ => HttpMethodType.GET
            };
        }

        private static string CombineUrls(string? baseUrl, string path)
        {
            if (string.IsNullOrEmpty(baseUrl))
                return path;

            var trimmedBase = baseUrl.TrimEnd('/');
            var trimmedPath = path.TrimStart('/');
            
            return $"{trimmedBase}/{trimmedPath}";
        }
    }
} 