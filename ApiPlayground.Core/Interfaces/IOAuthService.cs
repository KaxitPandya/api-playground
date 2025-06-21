using ApiPlayground.Core.Models;

namespace ApiPlayground.Core.Interfaces
{
    public interface IOAuthService
    {
        Task<string> GetAuthorizationUrlAsync(string integrationId, OAuth2Config config, string state = "");
        Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string integrationId, string code, string state);
        Task<OAuthTokenResponse> RefreshTokenAsync(string integrationId, string refreshToken);
        Task<bool> IsTokenValidAsync(string integrationId);
        Task RevokeTokenAsync(string integrationId);
        Task<string?> GetValidAccessTokenAsync(string integrationId);
    }
} 