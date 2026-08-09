using CafeAutomate.Api.Data;
using CafeAutomate.Api.DTOs;
using CafeAutomate.Api.Middleware;
using CafeAutomate.Api.Models;
using CafeAutomate.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeAutomate.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;

    public AuthController(AppDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.FullName) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "All fields are required." });

        if (req.Password.Length < 6)
            return BadRequest(new { error = "Password must be at least 6 characters." });

        if (!req.Email.Trim().ToLower().EndsWith("@stewart.com"))
            return BadRequest(new { error = "Only @stewart.com email addresses are allowed to register." });

        if (await _db.Users.AnyAsync(u => u.Email == req.Email.ToLower()))
            return Conflict(new { error = "An account with this email already exists." });

        var user = new User
        {
            FullName = req.FullName.Trim(),
            Email = req.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = UserRole.User
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokens.Generate(user);
        return Ok(new AuthResponse(token, user.Id, user.FullName, user.Email, (int)user.Role, user.Role.ToString()));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "Email and password are required." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower().Trim());

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { error = "Invalid email or password." });

        if (!user.IsActive)
            return Unauthorized(new { error = "Your account has been disabled. Contact support." });

        var token = _tokens.Generate(user);
        return Ok(new AuthResponse(token, user.Id, user.FullName, user.Email, (int)user.Role, user.Role.ToString()));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId();
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return Ok(new MeResponse(user.Id, user.FullName, user.Email, (int)user.Role, user.Role.ToString(), user.IsActive));
    }
}
