namespace CafeAutomate.Api.DTOs;

public record CartItemRequest(
    string SourceType,
    int MenuItemId,
    string ItemName,
    decimal UnitPrice,
    int Quantity
);

public record CheckoutRequest(List<CartItemRequest> Items);

public record OrderItemResponse(
    int Id,
    string SourceType,
    int MenuItemId,
    string ItemName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal
);

public record OrderResponse(
    int Id,
    int UserId,
    string UserName,
    string UserEmail,
    string OrderStatus,
    string PaymentStatus,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<OrderItemResponse> Items
);

public record UpdateOrderStatusRequest(string Status);
public record UpdatePaymentStatusRequest(string Status);
