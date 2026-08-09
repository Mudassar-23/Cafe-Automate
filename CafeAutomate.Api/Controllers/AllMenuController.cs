using CafeAutomate.Api.Data;
using CafeAutomate.Api.DTOs;
using CafeAutomate.Api.Middleware;
using CafeAutomate.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeAutomate.Api.Controllers;

[ApiController]
[Route("api/all-menu")]
public class AllMenuController : ControllerBase
{
    private readonly AppDbContext _db;

    public AllMenuController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.AllMenuItems
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new AllMenuItemResponse(m.Id, m.Name, m.Description, m.Price, m.Emoji, m.Category, m.IsAvailable, m.CreatedAt))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] AllMenuItemRequest req)
    {
        if (!User.IsWebsiteAdmin()) return Forbid();

        var item = new AllMenuItem
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            Emoji = string.IsNullOrWhiteSpace(req.Emoji) ? "☕" : req.Emoji,
            Category = req.Category,
            IsAvailable = req.IsAvailable
        };

        _db.AllMenuItems.Add(item);
        await _db.SaveChangesAsync();

        return Created($"/api/all-menu/{item.Id}",
            new AllMenuItemResponse(item.Id, item.Name, item.Description, item.Price, item.Emoji, item.Category, item.IsAvailable, item.CreatedAt));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] AllMenuItemRequest req)
    {
        if (!User.IsWebsiteAdmin()) return Forbid();

        var item = await _db.AllMenuItems.FindAsync(id);
        if (item == null) return NotFound();

        item.Name = req.Name;
        item.Description = req.Description;
        item.Price = req.Price;
        item.Emoji = string.IsNullOrWhiteSpace(req.Emoji) ? "☕" : req.Emoji;
        item.Category = req.Category;
        item.IsAvailable = req.IsAvailable;

        await _db.SaveChangesAsync();
        return Ok(new AllMenuItemResponse(item.Id, item.Name, item.Description, item.Price, item.Emoji, item.Category, item.IsAvailable, item.CreatedAt));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        if (!User.IsWebsiteAdmin()) return Forbid();

        var item = await _db.AllMenuItems.FindAsync(id);
        if (item == null) return NotFound();

        _db.AllMenuItems.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
