using Logging.Jwt.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Logging.Jwt.Web.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    JwtTokenService tokenService,
    AuthCookieService authCookieService)
{
    public async Task<(string Token, DateTime ExpiresAt, string Email, string? DisplayName)?> TryLoginAsync(
        string email,
        string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return null;
        }

        var (token, expiresAt) = tokenService.CreateToken(user);
        authCookieService.SetAccessTokenCookie(token, expiresAt);
        return (token, expiresAt, user.Email ?? email, user.DisplayName);
    }
}
