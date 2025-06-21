using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ApiPlayground.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OAuthController : ControllerBase
    {
        private readonly IOAuthService _oauthService;
        private readonly IIntegrationService _integrationService;
        private readonly ILogger<OAuthController> _logger;

        public OAuthController(
            IOAuthService oauthService, 
            IIntegrationService integrationService,
            ILogger<OAuthController> logger)
        {
            _oauthService = oauthService;
            _integrationService = integrationService;
            _logger = logger;
        }

        /// <summary>
        /// Get OAuth authorization URL for an integration (Frontend compatible endpoint)
        /// </summary>
        /// <param name="request">OAuth authorization request</param>
        /// <returns>Authorization URL</returns>
        [HttpPost("authorize")]
        public async Task<ActionResult<string>> StartOAuthFlow([FromBody] OAuthAuthorizationRequest request)
        {
            try
            {
                _logger.LogInformation("Getting OAuth authorization URL for integration: {IntegrationId}", request.IntegrationId);

                // Return a local demo OAuth URL instead of trying to connect to GitHub with invalid credentials
                var state = request.State ?? Guid.NewGuid().ToString();
                var authUrl = $"http://localhost:3000/oauth/demo?state={state}&integration_id={request.IntegrationId}";

                return Ok(authUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OAuth authorization URL for integration: {IntegrationId}", request.IntegrationId);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Exchange OAuth code for token (Frontend compatible endpoint)
        /// </summary>
        /// <param name="request">OAuth token exchange request</param>
        /// <returns>Token response</returns>
        [HttpPost("token")]
        public async Task<ActionResult<OAuthTokenResponse>> ExchangeCodeForToken([FromBody] OAuthTokenExchangeRequest request)
        {
            try
            {
                _logger.LogInformation("Exchanging OAuth code for token for integration: {IntegrationId}", request.IntegrationId);

                // For now, return a mock token response
                var tokenResponse = new OAuthTokenResponse
                {
                    AccessToken = "mock_access_token_" + Guid.NewGuid().ToString("N")[..16],
                    TokenType = "Bearer",
                    ExpiresIn = 3600,
                    RefreshToken = "mock_refresh_token_" + Guid.NewGuid().ToString("N")[..16],
                    Scope = new List<string> { "repo", "user" }
                };

                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging OAuth code for token for integration: {IntegrationId}", request.IntegrationId);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Refresh OAuth token (Frontend compatible endpoint)
        /// </summary>
        /// <param name="request">OAuth refresh request</param>
        /// <returns>New token response</returns>
        [HttpPost("refresh")]
        public async Task<ActionResult<OAuthTokenResponse>> RefreshOAuthToken([FromBody] OAuthRefreshRequest request)
        {
            try
            {
                _logger.LogInformation("Refreshing OAuth token for integration: {IntegrationId}", request.IntegrationId);

                // For now, return a mock refreshed token
                var tokenResponse = new OAuthTokenResponse
                {
                    AccessToken = "refreshed_access_token_" + Guid.NewGuid().ToString("N")[..16],
                    TokenType = "Bearer",
                    ExpiresIn = 3600,
                    RefreshToken = request.RefreshToken, // Keep the same refresh token
                    Scope = new List<string> { "repo", "user" }
                };

                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing OAuth token for integration: {IntegrationId}", request.IntegrationId);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Validate OAuth token (Frontend compatible endpoint)
        /// </summary>
        /// <param name="integrationId">Integration ID</param>
        /// <returns>Token validity status</returns>
        [HttpGet("validate/{integrationId}")]
        public async Task<ActionResult<bool>> ValidateToken(string integrationId)
        {
            try
            {
                _logger.LogInformation("Validating OAuth token for integration: {IntegrationId}", integrationId);

                // For now, return true as we're using mock tokens
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OAuth token for integration: {IntegrationId}", integrationId);
                return StatusCode(500, false);
            }
        }

        /// <summary>
        /// Revoke OAuth token (Frontend compatible endpoint)
        /// </summary>
        /// <param name="integrationId">Integration ID</param>
        /// <returns>Success response</returns>
        [HttpPost("revoke/{integrationId}")]
        public async Task<ActionResult> RevokeOAuthToken(string integrationId)
        {
            try
            {
                _logger.LogInformation("Revoking OAuth token for integration: {IntegrationId}", integrationId);

                // Mock revocation - always successful
                return Ok(new { message = "Token revoked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking OAuth token for integration: {IntegrationId}", integrationId);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Get OAuth authorization URL for an integration (alternative endpoint)
        /// </summary>
        /// <param name="integrationId">Integration ID</param>
        /// <param name="redirectUri">Optional redirect URI override</param>
        /// <returns>Authorization URL</returns>
        [HttpGet("authorize/{integrationId}")]
        public async Task<ActionResult<object>> GetAuthorizationUrlByGet(string integrationId, [FromQuery] string? redirectUri = null)
        {
            try
            {
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                
                if (integration?.Authentication?.OAuth2 == null)
                {
                    // Return mock data for demo purposes
                    var state = Guid.NewGuid().ToString();
                    var authUrl = $"https://github.com/login/oauth/authorize?client_id=your_client_id&redirect_uri=http://localhost:3000/oauth/callback&scope=repo%20user&state={state}";
                    
                    return Ok(new { 
                        authorizationUrl = authUrl,
                        state = state
                    });
                }

                var config = integration.Authentication.OAuth2;
                
                // Use provided redirect URI or default from config
                if (!string.IsNullOrEmpty(redirectUri))
                {
                    config.RedirectUri = redirectUri;
                }

                var stateParam = Guid.NewGuid().ToString();
                var authorizationUrl = await _oauthService.GetAuthorizationUrlAsync(integrationId, config, stateParam);

                _logger.LogInformation("Generated OAuth authorization URL for integration: {IntegrationId}", integrationId);

                return Ok(new { 
                    authorizationUrl = authorizationUrl,
                    state = stateParam
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OAuth authorization URL for integration: {IntegrationId}", integrationId);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Handle OAuth callback and exchange code for token
        /// </summary>
        /// <param name="code">Authorization code</param>
        /// <param name="state">State parameter</param>
        /// <param name="integrationId">Integration ID</param>
        /// <returns>Token response</returns>
        [HttpGet("callback")]
        public async Task<ActionResult<OAuthTokenResponse>> HandleCallback(
            [FromQuery] string code, 
            [FromQuery] string state, 
            [FromQuery] string integrationId)
        {
            try
            {
                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(integrationId))
                {
                    return BadRequest("Code and integration ID are required");
                }

                _logger.LogInformation("Handling OAuth callback for integration: {IntegrationId}", integrationId);

                var tokenResponse = await _oauthService.ExchangeCodeForTokenAsync(integrationId, code, state);

                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling OAuth callback for integration: {IntegrationId}", integrationId);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Check if OAuth token is valid for an integration
        /// </summary>
        /// <param name="integrationId">Integration ID</param>
        /// <returns>Token validity status</returns>
        [HttpGet("status/{integrationId}")]
        public async Task<ActionResult<object>> GetTokenStatus(string integrationId)
        {
            try
            {
                var isValid = await _oauthService.IsTokenValidAsync(integrationId);
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                
                var response = new
                {
                    isValid = isValid,
                    hasRefreshToken = !string.IsNullOrEmpty(integration?.Authentication?.OAuth2?.RefreshToken),
                    expiresAt = integration?.Authentication?.OAuth2?.TokenExpiry
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking OAuth token status for integration: {IntegrationId}", integrationId);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Configure OAuth2 settings for an integration
        /// </summary>
        /// <param name="integrationId">Integration ID</param>
        /// <param name="config">OAuth2 configuration</param>
        /// <returns>Success response</returns>
        [HttpPost("configure/{integrationId}")]
        public async Task<ActionResult> ConfigureOAuth(string integrationId, [FromBody] OAuth2Config config)
        {
            try
            {
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                
                if (integration == null)
                {
                    return NotFound("Integration not found");
                }

                // Initialize authentication if not exists
                if (integration.Authentication == null)
                {
                    integration.Authentication = new AuthenticationConfig();
                }

                integration.Authentication.Type = AuthenticationType.OAuth2;
                integration.Authentication.OAuth2 = config;

                await _integrationService.UpdateIntegrationAsync(integration);

                _logger.LogInformation("OAuth2 configuration updated for integration: {IntegrationId}", integrationId);

                return Ok(new { message = "OAuth2 configuration updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring OAuth for integration: {IntegrationId}", integrationId);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Get OAuth2 configuration for an integration (without sensitive data)
        /// </summary>
        /// <param name="integrationId">Integration ID</param>
        /// <returns>OAuth2 configuration</returns>
        [HttpGet("config/{integrationId}")]
        public async Task<ActionResult<object>> GetOAuthConfig(string integrationId)
        {
            try
            {
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                
                if (integration?.Authentication?.OAuth2 == null)
                {
                    return NotFound("Integration not found or OAuth2 not configured");
                }

                var config = integration.Authentication.OAuth2;
                
                // Return config without sensitive data
                var safeConfig = new
                {
                    clientId = config.ClientId,
                    authorizationUrl = config.AuthorizationUrl,
                    tokenUrl = config.TokenUrl,
                    redirectUri = config.RedirectUri,
                    scopes = config.Scopes,
                    hasAccessToken = !string.IsNullOrEmpty(config.AccessToken),
                    hasRefreshToken = !string.IsNullOrEmpty(config.RefreshToken),
                    tokenExpiry = config.TokenExpiry
                };

                return Ok(safeConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OAuth config for integration: {IntegrationId}", integrationId);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }

    // Request models for frontend compatibility
    public class OAuthAuthorizationRequest
    {
        public string IntegrationId { get; set; } = string.Empty;
        public string? State { get; set; }
    }

    public class OAuthTokenExchangeRequest
    {
        public string IntegrationId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? State { get; set; }
    }

    public class OAuthRefreshRequest
    {
        public string IntegrationId { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
} 