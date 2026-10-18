namespace PizzaShop.Api.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int TableNumber { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = "Unpaid";
    public string OrderStatus { get; set; } = "Pending";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
}
