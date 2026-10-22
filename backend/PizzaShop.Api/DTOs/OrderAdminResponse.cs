namespace PizzaShop.Api.DTOs;

public sealed class OrderAdminResponse
{
    public Guid Id { get; init; }
    public int TableNumber { get; init; }
    public decimal Total { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public string OrderStatus { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public IReadOnlyList<OrderAdminItemResponse> Items { get; init; } = [];
}

public sealed class OrderAdminItemResponse
{
    public int MenuItemId { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal LineTotal { get; init; }
}
