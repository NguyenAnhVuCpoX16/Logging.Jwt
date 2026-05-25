namespace Logging.Jwt.Web.Models;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, DateTime ExpiresAt, string Email, string? DisplayName);

public record CreateUserRequest(string Email, string Password, string? DisplayName);

public record CreateUserResponse(string UserId, string Email);
