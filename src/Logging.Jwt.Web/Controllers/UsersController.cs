using Logging.Jwt.Data.Entities;
using Logging.Jwt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Logging.Jwt.Web.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateUserResponse>> Create([FromBody] CreateUserRequest request)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return Conflict(new { message = "Email already exists." });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                errors = result.Errors.Select(e => e.Description)
            });
        }

        return CreatedAtAction(nameof(Create), new { id = user.Id }, new CreateUserResponse(user.Id, user.Email ?? request.Email));
    }
}
