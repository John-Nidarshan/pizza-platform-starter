using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Api.DTOs;

public class OrderItemRequest
{
    [Range(1, int.MaxValue)]
    public int MenuItemId { get; set; }

    [Range(1, 20)]
    public int Quantity { get; set; }
}
