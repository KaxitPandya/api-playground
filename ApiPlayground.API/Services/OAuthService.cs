using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using System.Text;
using System.Text.Json;
using System.Web;

namespace ApiPlayground.API.Services
{
    public class OAuthService : IOAuthService
    {
        private readonly IIntegrationService _integrationService;
        private readonly ILogger<OAuthService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OAuthService(
            IIntegrationService integrationService,
            ILogger<OAuthService> logger,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _integrationService = integrationService;
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetAuthorizationUrlAsync(string integrationId, OAuth2Config config, string state = "")
        {
            try
            {
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                if (integration == null)
                    throw new ArgumentException("Integration not found");

                var stateValue = string.IsNullOrEmpty(state) ? Guid.NewGuid().ToString() : state;
                
                var queryParams = new Dictionary<string, string>
                {
                    ["client_id"] = config.ClientId,
                    ["redirect_uri"] = config.RedirectUri,
                    ["response_type"] = "code",
                    ["state"] = stateValue,
                    ["scope"] = string.Join(" ", config.Scopes)
                };

                var queryString = string.Join("&", queryParams.Select(kvp => 
                    $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"));

                var authUrl = $"{config.AuthorizationUrl}?{queryString}";
                
                _logger.LogInformation("Generated authorization URL for integration {IntegrationId}: {AuthUrl}", integrationId, authUrl);
                
                return authUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating authorization URL for integration: {IntegrationId}", integrationId);
                throw;
            }
        }

        public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string integrationId, string code, string state)
        {
            try
            {
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                if (integration?.Authentication?.OAuth2 == null)
                    throw new ArgumentException("Integration or OAuth configuration not found");

                var config = integration.Authentication.OAuth2;
                
                var tokenRequest = new Dictionary<string, string>
                {
                    ["grant_type"] = "authorization_code",
                    ["client_id"] = config.ClientId,
                    ["client_secret"] = config.ClientSecret,
                    ["code"] = code,
                    ["redirect_uri"] = config.RedirectUri
                };

                var content = new FormUrlEncodedContent(tokenRequest);
                var response = await _httpClient.PostAsync(config.TokenUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Token exchange failed: {response.StatusCode} - {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

                var tokenResponse = new OAuthTokenResponse
                {
                    AccessToken = tokenData.GetValueOrDefault("access_token")?.ToString() ?? "",
                    RefreshToken = tokenData.GetValueOrDefault("refresh_token")?.ToString(),
                    ExpiresIn = int.TryParse(tokenData.GetValueOrDefault("expires_in")?.ToString(), out var expires) ? expires : 3600,
                    TokenType = tokenData.GetValueOrDefault("token_type")?.ToString() ?? "Bearer",
                    Scope = tokenData.GetValueOrDefault("scope")?.ToString()?.Split(' ').ToList() ?? new List<string>()
                };

                // Update the integration with the new tokens
                config.AccessToken = tokenResponse.AccessToken;
                config.RefreshToken = tokenResponse.RefreshToken;
                config.TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                
                await _integrationService.UpdateIntegrationAsync(integration);

                _logger.LogInformation("Successfully exchanged code for token for integration: {IntegrationId}", integrationId);
                
                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging code for token for integration: {IntegrationId}", integrationId);
                throw;
            }
        }

        public async Task<OAuthTokenResponse> RefreshTokenAsync(string integrationId, string refreshToken)
        {
            try
            {
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                if (integration?.Authentication?.OAuth2 == null)
                    throw new ArgumentException("Integration or OAuth configuration not found");

                var config = integration.Authentication.OAuth2;
                
                var tokenRequest = new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = config.ClientId,
                    ["client_secret"] = config.ClientSecret,
                    ["refresh_token"] = refreshToken
                };

                var content = new FormUrlEncodedContent(tokenRequest);
                var response = await _httpClient.PostAsync(config.TokenUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Token refresh failed: {response.StatusCode} - {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

                var tokenResponse = new OAuthTokenResponse
                {
                    AccessToken = tokenData.GetValueOrDefault("access_token")?.ToString() ?? "",
                    RefreshToken = tokenData.GetValueOrDefault("refresh_token")?.ToString() ?? refreshToken,
                    ExpiresIn = int.TryParse(tokenData.GetValueOrDefault("expires_in")?.ToString(), out var expires) ? expires : 3600,
                    TokenType = tokenData.GetValueOrDefault("token_type")?.ToString() ?? "Bearer",
                    Scope = tokenData.GetValueOrDefault("scope")?.ToString()?.Split(' ').ToList() ?? new List<string>()
                };

                // Update the integration with the new tokens
                config.AccessToken = tokenResponse.AccessToken;
                config.RefreshToken = tokenResponse.RefreshToken;
                config.TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                
                await _integrationService.UpdateIntegrationAsync(integration);

                _logger.LogInformation("Successfully refreshed token for integration: {IntegrationId}", integrationId);
                
                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token for integration: {IntegrationId}", integrationId);
                throw;
            }
        }

        public async Task<bool> IsTokenValidAsync(string integrationId)
        {
            try
            {
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                if (integration?.Authentication?.OAuth2 == null)
                    return false;

                var config = integration.Authentication.OAuth2;
                
                // Check if token exists and is not expired
                if (string.IsNullOrEmpty(config.AccessToken))
                    return false;

                if (config.TokenExpiry.HasValue && config.TokenExpiry.Value <= DateTime.UtcNow.AddMinutes(5))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token validity for integration: {IntegrationId}", integrationId);
                return false;
            }
        }

        public async Task RevokeTokenAsync(string integrationId)
        {
            try
            {
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                if (integration?.Authentication?.OAuth2 == null)
                    return;

                var config = integration.Authentication.OAuth2;
                
                // Clear tokens from the configuration
                config.AccessToken = null;
                config.RefreshToken = null;
                config.TokenExpiry = null;
                
                await _integrationService.UpdateIntegrationAsync(integration);
                
                _logger.LogInformation("Revoked tokens for integration: {IntegrationId}", integrationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token for integration: {IntegrationId}", integrationId);
                throw;
            }
        }

        public async Task<string?> GetValidAccessTokenAsync(string integrationId)
        {
            try
            {
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                if (integration?.Authentication?.OAuth2 == null)
                    return null;

                var config = integration.Authentication.OAuth2;
                
                // Check if current token is valid
                if (await IsTokenValidAsync(integrationId))
                    return config.AccessToken;

                // Try to refresh token if refresh token is available
                if (!string.IsNullOrEmpty(config.RefreshToken))
                {
                    try
                    {
                        var refreshResponse = await RefreshTokenAsync(integrationId, config.RefreshToken);
                        return refreshResponse.AccessToken;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to refresh token for integration: {IntegrationId}", integrationId);
                        return null;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting valid access token for integration: {IntegrationId}", integrationId);
                return null;
            }
        }
    }
} 