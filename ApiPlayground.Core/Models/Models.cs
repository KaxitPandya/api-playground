using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiPlayground.Core.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HttpMethodType
    {
        GET,
        POST,
        PUT,
        DELETE,
        PATCH,
        HEAD,
        OPTIONS
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AuthenticationType
    {
        None,
        BearerToken,
        OAuth2,
        BasicAuth,
        ApiKey
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExecutionMode
    {
        Sequential,
        Parallel,
        Conditional
    }

    public class Integration
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ExecutionMode ExecutionMode { get; set; } = ExecutionMode.Sequential;
        
        public AuthenticationConfig? Authentication { get; set; }
        
        // Navigation property for Entity Framework
        public List<Request> Requests { get; set; } = new List<Request>();
    }

    public class Request
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string IntegrationId { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public HttpMethodType Method { get; set; } = HttpMethodType.GET;
        
        [Required]
        public string Url { get; set; } = string.Empty;
        
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        
        public string? Body { get; set; }
        
        public int Order { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Advanced execution features
        public RetryConfig? RetryConfig { get; set; }
        
        public List<ConditionalRule> ConditionalRules { get; set; } = new List<ConditionalRule>();
        
        public bool CanRunInParallel { get; set; } = true;
        
        public List<string> DependsOn { get; set; } = new List<string>(); // Request IDs this depends on
        
        // Navigation property for Entity Framework
        public Integration? Integration { get; set; }
    }

    public class RequestResult
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string RequestId { get; set; } = string.Empty;
        
        public string RequestName { get; set; } = string.Empty;
        
        public int StatusCode { get; set; }
        
        public long ResponseTimeMs { get; set; }
        
        public long ExecutionTimeMs { get; set; }
        
        public string? Response { get; set; }
        
        public string? Error { get; set; }
        
        public int AttemptNumber { get; set; } = 1;
        
        public bool IsRetry { get; set; } = false;
        
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public Request? Request { get; set; }
    }

    public class AuthenticationConfig
    {
        public AuthenticationType Type { get; set; } = AuthenticationType.None;
        
        public string? BearerToken { get; set; }
        
        public OAuth2Config? OAuth2 { get; set; }
        
        public BasicAuthConfig? BasicAuth { get; set; }
        
        public ApiKeyConfig? ApiKey { get; set; }
    }

    public class OAuth2Config
    {
        public string ClientId { get; set; } = string.Empty;
        
        public string ClientSecret { get; set; } = string.Empty;
        
        public string AuthorizationUrl { get; set; } = string.Empty;
        
        public string TokenUrl { get; set; } = string.Empty;
        
        public string RedirectUri { get; set; } = string.Empty;
        
        public List<string> Scopes { get; set; } = new List<string>();
        
        public string? AccessToken { get; set; }
        
        public string? RefreshToken { get; set; }
        
        public DateTime? TokenExpiry { get; set; }
    }

    public class BasicAuthConfig
    {
        public string Username { get; set; } = string.Empty;
        
        public string Password { get; set; } = string.Empty;
    }

    public class ApiKeyConfig
    {
        public string Key { get; set; } = string.Empty;
        
        public string Value { get; set; } = string.Empty;
        
        public string Location { get; set; } = "header"; // header, query, cookie
    }

    public class RetryConfig
    {
        public int MaxAttempts { get; set; } = 3;
        
        public int DelayMs { get; set; } = 1000;
        
        public bool ExponentialBackoff { get; set; } = true;
        
        public List<int> RetryOnStatusCodes { get; set; } = new List<int> { 500, 502, 503, 504 };
    }

    public class ConditionalRule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string Condition { get; set; } = string.Empty; // JSONPath expression
        
        public string ExpectedValue { get; set; } = string.Empty;
        
        public string Operator { get; set; } = "equals"; // equals, contains, greater_than, etc.
        
        public string Action { get; set; } = "continue"; // continue, skip, stop, retry
        
        public string? TargetRequestId { get; set; } // For conditional branching
    }

    public class PlaceholderMap
    {
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
    }

    // AI Generation Models
    public class AIGenerationRequest
    {
        public string Description { get; set; } = string.Empty;
        
        public string? BaseUrl { get; set; }
        
        public List<string> Examples { get; set; } = new List<string>();
        
        public string? AuthenticationType { get; set; }
    }

    public class AIGenerationResponse
    {
        public Integration Integration { get; set; } = new Integration();
        
        public string Explanation { get; set; } = string.Empty;
        
        public List<string> Suggestions { get; set; } = new List<string>();
    }

    // OpenAPI Import Models
    public class OpenAPIImportRequest
    {
        public string? Url { get; set; }
        
        public string? FileContent { get; set; }
        
        public List<string> SelectedOperations { get; set; } = new List<string>();
        
        public string? BaseUrl { get; set; }
    }

    public class OpenAPIImportResponse
    {
        public Integration Integration { get; set; } = new Integration();
        
        public List<string> AvailableOperations { get; set; } = new List<string>();
        
        public int ImportedCount { get; set; }
        
        public List<string> Warnings { get; set; } = new List<string>();
    }

    // Execution Configuration
    public class ExecutionConfig
    {
        public Dictionary<string, string> Placeholders { get; set; } = new Dictionary<string, string>();
        
        public ExecutionMode Mode { get; set; } = ExecutionMode.Sequential;
        
        public int MaxParallelRequests { get; set; } = 5;
        
        public int TimeoutMs { get; set; } = 30000;
        
        public bool StopOnFirstError { get; set; } = false;
        
        public bool EnableRetries { get; set; } = true;
    }

    // OAuth Flow Models
    public class OAuthAuthorizationRequest
    {
        public string IntegrationId { get; set; } = string.Empty;
        
        public string State { get; set; } = string.Empty;
    }

    public class OAuthTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        
        public string? RefreshToken { get; set; }
        
        public int ExpiresIn { get; set; }
        
        public string TokenType { get; set; } = "Bearer";
        
        public List<string> Scope { get; set; } = new List<string>();
    }

    // Execution Request Models
    public class ParallelExecutionRequest
    {
        public Dictionary<string, string> Placeholders { get; set; } = new Dictionary<string, string>();
        
        public int MaxParallelRequests { get; set; } = 5;
        
        public int TimeoutMs { get; set; } = 30000;
        
        public bool EnableRetries { get; set; } = true;
    }

    public class ConditionalExecutionRequest
    {
        public Dictionary<string, string> Placeholders { get; set; } = new Dictionary<string, string>();
        
        public bool StopOnFirstError { get; set; } = false;
        
        public bool EnableRetries { get; set; } = true;
    }
}