namespace PizzaShop.Api.Models;

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string Emoji { get; set; } = "🍕";
    public bool IsAvailable { get; set; } = true;
}
