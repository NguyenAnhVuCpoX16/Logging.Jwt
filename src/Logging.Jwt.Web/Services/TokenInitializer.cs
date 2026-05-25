using Logging.Jwt.Web.Constants;

namespace Logging.Jwt.Web.Services;

public class TokenInitializer(IHttpContextAccessor httpContextAccessor, TokenStorage tokenStorage)
{
    public void InitializeFromCookie()
    {
        if (!string.IsNullOrWhiteSpace(tokenStorage.AccessToken))
        {
            return;
        }

        if (httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue(
                AuthConstants.AccessTokenCookieName, out var token) == true
            && !string.IsNullOrWhiteSpace(token))
        {
            tokenStorage.SetToken(token);
        }
    }
}
