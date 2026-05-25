using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Logging.Jwt.Web.Services;

public class JwtAuthenticationStateProvider(TokenStorage tokenStorage) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = CreatePrincipal(tokenStorage.AccessToken);
        return Task.FromResult(new AuthenticationState(principal));
    }

    public void NotifyAuthenticationStateChanged()
    {
        var principal = CreatePrincipal(tokenStorage.AccessToken);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    private static ClaimsPrincipal CreatePrincipal(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            var identity = new ClaimsIdentity(jwt.Claims, authenticationType: "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}
