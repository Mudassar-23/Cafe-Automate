using CafeAutomate.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace CafeAutomate.Api.Hubs;

public class OrderHub : Hub
{
    public async Task JoinOrderGroup(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    public async Task JoinCafeAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "cafe-admin");
    }

    public async Task JoinWebsiteAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "website-admin");
    }

    public async Task LeaveOrderGroup(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }
}
