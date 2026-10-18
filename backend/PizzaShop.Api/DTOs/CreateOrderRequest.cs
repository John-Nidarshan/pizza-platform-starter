using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Api.DTOs;

public class CreateOrderRequest
{
    [Range(1, 1000)]
    public int TableNumber { get; set; }

    [Required, MinLength(1), MaxLength(50)]
    public List<OrderItemRequest> Items { get; set; } = [];

    [Required]
    [RegularExpression("^(online|counter)$", ErrorMessage = "PaymentMethod must be 'online' or 'counter'.")]
    public string PaymentMethod { get; set; } = string.Empty;
}
