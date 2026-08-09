namespace CafeAutomate.Api.DTOs;

public record AllMenuItemRequest(
    string Name,
    string Description,
    decimal Price,
    string Emoji,
    string Category,
    bool IsAvailable
);

public record AllMenuItemResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string Emoji,
    string Category,
    bool IsAvailable,
    DateTime CreatedAt
);

public record DailyMenuItemRequest(
    string Name,
    decimal Price,
    int Quantity
);

public record DailyMenuItemResponse(
    int Id,
    string Name,
    decimal Price,
    int Quantity,
    string Status,
    string Emoji,
    string Date,
    DateTime CreatedAt
);
