using Microsoft.AspNetCore.Identity;

namespace Logging.Jwt.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
