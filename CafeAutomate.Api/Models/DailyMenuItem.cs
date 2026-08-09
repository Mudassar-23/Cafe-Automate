namespace CafeAutomate.Api.Models;

public enum DailyItemStatus { Available, SoldOut }

public class DailyMenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DailyItemStatus Status { get; set; } = DailyItemStatus.Available;
    public string Emoji { get; set; } = "☕";
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
