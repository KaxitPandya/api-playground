using ApiPlayground.Core.Interfaces;
using ApiPlayground.Core.Models;
using Microsoft.AspNetCore.Mvc;

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
        /// Get OAuth authorization URL for an integration
        /// </summary>
        /// <param name="integrationId">Integration ID</param>
        /// <param name="redirectUri">Optional redirect URI override</param>
        /// <returns>Authorization URL</returns>
        [HttpGet("authorize/{integrationId}")]
        public async Task<ActionResult<object>> GetAuthorizationUrl(string integrationId, [FromQuery] string? redirectUri = null)
        {
            try
            {
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                
                if (integration?.Authentication?.OAuth2 == null)
                {
                    return BadRequest("Integration not found or OAuth2 not configured");
                }

                var config = integration.Authentication.OAuth2;
                
                // Use provided redirect URI or default from config
                if (!string.IsNullOrEmpty(redirectUri))
                {
                    config.RedirectUri = redirectUri;
                }

                var state = Guid.NewGuid().ToString();
                var authUrl = await _oauthService.GetAuthorizationUrlAsync(integrationId, config, state);

                _logger.LogInformation("Generated OAuth authorization URL for integration: {IntegrationId}", integrationId);

                return Ok(new { 
                    authorizationUrl = authUrl,
                    state = state
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
        /// Refresh OAuth token for an integration
        /// </summary>
        /// <param name="integrationId">Integration ID</param>
        /// <returns>New token response</returns>
        [HttpPost("refresh/{integrationId}")]
        public async Task<ActionResult<OAuthTokenResponse>> RefreshToken(string integrationId)
        {
            try
            {
                var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);
                
                if (integration?.Authentication?.OAuth2?.RefreshToken == null)
                {
                    return BadRequest("Integration not found or refresh token not available");
                }

                _logger.LogInformation("Refreshing OAuth token for integration: {IntegrationId}", integrationId);

                var tokenResponse = await _oauthService.RefreshTokenAsync(integrationId, integration.Authentication.OAuth2.RefreshToken);

                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing OAuth token for integration: {IntegrationId}", integrationId);
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
        /// Revoke OAuth token for an integration
        /// </summary>
        /// <param name="integrationId">Integration ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("revoke/{integrationId}")]
        public async Task<ActionResult> RevokeToken(string integrationId)
        {
            try
            {
                _logger.LogInformation("Revoking OAuth token for integration: {IntegrationId}", integrationId);

                await _oauthService.RevokeTokenAsync(integrationId);

                return Ok(new { message = "Token revoked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking OAuth token for integration: {IntegrationId}", integrationId);
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
} 