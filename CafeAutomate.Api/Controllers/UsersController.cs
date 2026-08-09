using CafeAutomate.Api.Data;
using CafeAutomate.Api.DTOs;
using CafeAutomate.Api.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeAutomate.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!User.IsWebsiteAdmin()) return Forbid();

        var users = await _db.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserResponse(u.Id, u.FullName, u.Email, (int)u.Role, u.Role.ToString(), u.IsActive, u.CreatedAt))
            .ToListAsync();

        return Ok(users);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateUserStatusRequest req)
    {
        if (!User.IsWebsiteAdmin()) return Forbid();

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = req.IsActive;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"User {(req.IsActive ? "enabled" : "disabled")} successfully." });
    }

    [HttpPatch("{id}/password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangeUserPasswordRequest req)
    {
        if (!User.IsWebsiteAdmin()) return Forbid();

        if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 6)
            return BadRequest(new { error = "Password must be at least 6 characters." });

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound(new { error = "This user no longer exists." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Password updated for {user.FullName}." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!User.IsWebsiteAdmin()) return Forbid();

        if (id == User.GetUserId())
            return BadRequest(new { error = "You cannot delete your own account." });

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound(new { error = "This user no longer exists." });

        // Orders (and their items) cascade-delete with the user.
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = $"{user.FullName} deleted." });
    }
}
