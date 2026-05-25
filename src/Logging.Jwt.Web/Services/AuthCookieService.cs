using Logging.Jwt.Web.Constants;

namespace Logging.Jwt.Web.Services;

public class AuthCookieService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
{
    public void SetAccessTokenCookie(string token, DateTime expiresAt)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        httpContext.Response.Cookies.Append(
            AuthConstants.AccessTokenCookieName,
            token,
            CreateCookieOptions(httpContext, expiresAt));
    }

    public void ClearAccessTokenCookie()
    {
        if (httpContextAccessor.HttpContext is { } httpContext)
        {
            httpContext.Response.Cookies.Delete(AuthConstants.AccessTokenCookieName, new CookieOptions { Path = "/" });
        }
    }

    private CookieOptions CreateCookieOptions(HttpContext httpContext, DateTime expiresAt) => new()
    {
        HttpOnly = true,
        Secure = httpContext.Request.IsHttps || configuration.GetValue("Cookie:Secure", false),
        SameSite = SameSiteMode.Lax,
        Path = "/",
        Expires = expiresAt
    };
}
