using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace HoTeach
{
    public static class AuthHelper
    {
        public static ClaimsPrincipal ValidateToken(HttpRequest req)
        {
            if (!req.Headers.ContainsKey("Authorization"))
                throw new UnauthorizedAccessException("Authorization header missing.");

            var authHeader = req.Headers["Authorization"].ToString();

            if (!authHeader.StartsWith("Bearer "))
                throw new UnauthorizedAccessException("Invalid authorization header format.");

            var token = authHeader.Substring("Bearer ".Length).Trim();

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidIssuer = $"https://dev-m31s5020w8rygw8y.us.auth0.com/",
                ValidAudience = "https://hoteachaudience.com",
                IssuerSigningKeys = OpenIdConnectConfigurationRetriever
                    .GetAsync($"https://dev-m31s5020w8rygw8y.us.auth0.com/.well-known/openid-configuration", default)
                    .GetAwaiter().GetResult()
                    .SigningKeys
            };

            var principal = handler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        public static bool HasScope(ClaimsPrincipal principal, string scope)
        {
            var scopeClaim = principal.FindFirst("scope")?.Value;
            if (scopeClaim == null) return false;

            var scopes = scopeClaim.Split(' ');
            return scope.Split(' ').All(s => scopes.Contains(s, StringComparer.OrdinalIgnoreCase));
        }

    }
}
