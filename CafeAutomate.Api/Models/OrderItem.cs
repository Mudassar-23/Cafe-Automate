namespace CafeAutomate.Api.Models;

public enum MenuSourceType { DailyMenu, AllMenu }

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public MenuSourceType SourceType { get; set; }
    public int MenuItemId { get; set; }
    public string ItemNameSnapshot { get; set; } = string.Empty;
    public decimal UnitPriceSnapshot { get; set; }
    public int Quantity { get; set; }
}
