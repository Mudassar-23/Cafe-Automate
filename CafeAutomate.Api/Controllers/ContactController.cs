using CafeAutomate.Api.Data;
using CafeAutomate.Api.DTOs;
using CafeAutomate.Api.Middleware;
using CafeAutomate.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeAutomate.Api.Controllers;

[ApiController]
[Route("api/contact")]
public class ContactController : ControllerBase
{
    private readonly AppDbContext _db;

    public ContactController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] ContactMessageRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Message))
            return BadRequest(new { error = "All fields are required." });

        var msg = new ContactMessage
        {
            Name = req.Name.Trim(),
            Email = req.Email.Trim().ToLower(),
            Message = req.Message.Trim()
        };

        _db.ContactMessages.Add(msg);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Your message has been received. Thank you!" });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        if (!User.IsWebsiteAdmin()) return Forbid();

        var messages = await _db.ContactMessages
            .OrderByDescending(m => m.SubmittedAt)
            .Select(m => new ContactMessageResponse(m.Id, m.Name, m.Email, m.Message, m.SubmittedAt, m.IsRead))
            .ToListAsync();

        return Ok(messages);
    }

    [HttpPatch("{id}/read")]
    [Authorize]
    public async Task<IActionResult> MarkRead(int id)
    {
        if (!User.IsWebsiteAdmin()) return Forbid();

        var msg = await _db.ContactMessages.FindAsync(id);
        if (msg == null) return NotFound();

        msg.IsRead = true;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Marked as read." });
    }
}
