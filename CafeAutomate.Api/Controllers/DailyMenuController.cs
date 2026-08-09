using CafeAutomate.Api.Data;
using CafeAutomate.Api.DTOs;
using CafeAutomate.Api.Middleware;
using CafeAutomate.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeAutomate.Api.Controllers;

[ApiController]
[Route("api/daily-menu")]
public class DailyMenuController : ControllerBase
{
    private readonly AppDbContext _db;

    public DailyMenuController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetToday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var items = await _db.DailyMenuItems
            .Where(m => m.Date == today)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new DailyMenuItemResponse(m.Id, m.Name, m.Price, m.Quantity, m.Status.ToString(), m.Emoji, m.Date.ToString("yyyy-MM-dd"), m.CreatedAt))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] DailyMenuItemRequest req)
    {
        if (!User.IsCafeAdmin()) return Forbid();

        var item = new DailyMenuItem
        {
            Name = req.Name,
            Price = req.Price,
            Quantity = req.Quantity,
            Status = DailyItemStatus.Available,
            Date = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _db.DailyMenuItems.Add(item);
        await _db.SaveChangesAsync();

        return Created($"/api/daily-menu/{item.Id}",
            new DailyMenuItemResponse(item.Id, item.Name, item.Price, item.Quantity, item.Status.ToString(), item.Emoji, item.Date.ToString("yyyy-MM-dd"), item.CreatedAt));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] DailyMenuItemRequest req)
    {
        if (!User.IsCafeAdmin()) return Forbid();

        var item = await _db.DailyMenuItems.FindAsync(id);
        if (item == null) return NotFound(new { error = "This item no longer exists. The list has been refreshed." });

        item.Name = req.Name;
        item.Price = req.Price;
        item.Quantity = req.Quantity;

        await _db.SaveChangesAsync();
        return Ok(new DailyMenuItemResponse(item.Id, item.Name, item.Price, item.Quantity, item.Status.ToString(), item.Emoji, item.Date.ToString("yyyy-MM-dd"), item.CreatedAt));
    }

    [HttpPatch("{id}/status")]
    [Authorize]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        if (!User.IsCafeAdmin()) return Forbid();

        var item = await _db.DailyMenuItems.FindAsync(id);
        if (item == null) return NotFound();

        item.Status = item.Status == DailyItemStatus.Available ? DailyItemStatus.SoldOut : DailyItemStatus.Available;
        await _db.SaveChangesAsync();

        return Ok(new { id = item.Id, status = item.Status.ToString() });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        if (!User.IsCafeAdmin()) return Forbid();

        var item = await _db.DailyMenuItems.FindAsync(id);
        if (item == null) return NotFound();

        _db.DailyMenuItems.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
