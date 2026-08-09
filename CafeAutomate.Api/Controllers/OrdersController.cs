using CafeAutomate.Api.Data;
using CafeAutomate.Api.DTOs;
using CafeAutomate.Api.Hubs;
using CafeAutomate.Api.Middleware;
using CafeAutomate.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CafeAutomate.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<OrderHub> _hub;

    public OrdersController(AppDbContext db, IHubContext<OrderHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest req)
    {
        if (!User.IsUser()) return Forbid();
        if (req.Items == null || !req.Items.Any()) return BadRequest(new { error = "Cart is empty." });

        var userId = User.GetUserId();
        var order = new Order
        {
            UserId = userId,
            TotalAmount = req.Items.Sum(i => i.UnitPrice * i.Quantity),
            Items = req.Items.Select(i => new OrderItem
            {
                SourceType = Enum.Parse<MenuSourceType>(i.SourceType),
                MenuItemId = i.MenuItemId,
                ItemNameSnapshot = i.ItemName,
                UnitPriceSnapshot = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);
        var response = MapOrder(order, user!);

        await _hub.Clients.Group("cafe-admin").SendAsync("NewOrder", response);
        await _hub.Clients.Group("website-admin").SendAsync("NewOrder", response);

        return Created($"/api/orders/{order.Id}", response);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        if (!User.IsUser()) return Forbid();
        var userId = User.GetUserId();

        var orders = await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.User)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(o => MapOrder(o, o.User)));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!User.IsCafeAdmin() && !User.IsWebsiteAdmin()) return Forbid();

        var orders = await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(o => MapOrder(o, o.User)));
    }

    [HttpPatch("{id}/order-status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest req)
    {
        if (!User.IsCafeAdmin()) return Forbid();

        var order = await _db.Orders.Include(o => o.Items).Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();

        if (!Enum.TryParse<OrderStatus>(req.Status, out var status))
            return BadRequest(new { error = "Invalid order status." });

        order.OrderStatus = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var response = MapOrder(order, order.User);
        await _hub.Clients.Group($"order-{id}").SendAsync("OrderStatusUpdated", response);
        await _hub.Clients.Group("website-admin").SendAsync("OrderStatusUpdated", response);

        return Ok(response);
    }

    [HttpPatch("{id}/payment-status")]
    public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusRequest req)
    {
        if (!User.IsCafeAdmin()) return Forbid();

        var order = await _db.Orders.Include(o => o.Items).Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();

        if (!Enum.TryParse<PaymentStatus>(req.Status, out var status))
            return BadRequest(new { error = "Invalid payment status." });

        order.PaymentStatus = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var response = MapOrder(order, order.User);
        await _hub.Clients.Group($"order-{id}").SendAsync("PaymentStatusUpdated", response);
        await _hub.Clients.Group("website-admin").SendAsync("PaymentStatusUpdated", response);

        return Ok(response);
    }

    private static OrderResponse MapOrder(Order o, User user) => new(
        o.Id,
        o.UserId,
        user.FullName,
        user.Email,
        o.OrderStatus.ToString(),
        o.PaymentStatus.ToString(),
        o.TotalAmount,
        o.CreatedAt,
        o.UpdatedAt,
        o.Items.Select(i => new OrderItemResponse(
            i.Id,
            i.SourceType.ToString(),
            i.MenuItemId,
            i.ItemNameSnapshot,
            i.UnitPriceSnapshot,
            i.Quantity,
            i.UnitPriceSnapshot * i.Quantity
        )).ToList()
    );
}
