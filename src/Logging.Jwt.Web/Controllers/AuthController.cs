using Logging.Jwt.Web.Helpers;
using Logging.Jwt.Web.Models;
using Logging.Jwt.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logging.Jwt.Web.Controllers;

[ApiController]
public class AuthController(AuthService authService, AuthCookieService authCookieService) : ControllerBase
{
    [HttpPost("/auth/login")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> FormLogin(
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] string? returnUrl)
    {
        var loginResult = await authService.TryLoginAsync(email, password);
        if (loginResult is null)
        {
            var errorReturn = AuthUrlHelper.IsLocalUrl(returnUrl)
                ? $"&returnUrl={Uri.EscapeDataString(returnUrl!)}"
                : string.Empty;
            return Redirect($"/login?error=invalid{errorReturn}");
        }

        var destination = AuthUrlHelper.IsLocalUrl(returnUrl) ? returnUrl! : "/";
        return Redirect(destination);
    }

    [HttpGet("/auth/logout")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public IActionResult LogoutAndRedirect()
    {
        authCookieService.ClearAccessTokenCookie();
        return Redirect("/login");
    }

    [HttpPost("/api/auth/login")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<ActionResult<LoginResponse>> ApiLogin([FromBody] LoginRequest request)
    {
        var loginResult = await authService.TryLoginAsync(request.Email, request.Password);
        if (loginResult is null)
        {
            return Unauthorized();
        }

        var (token, expiresAt, email, displayName) = loginResult.Value;
        return Ok(new LoginResponse(token, expiresAt, email, displayName));
    }

    [HttpPost("/api/auth/logout")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public IActionResult ApiLogout()
    {
        authCookieService.ClearAccessTokenCookie();
        return Ok();
    }
}
