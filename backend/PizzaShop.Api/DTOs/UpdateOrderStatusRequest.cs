using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Api.DTOs;

public sealed class UpdateOrderStatusRequest
{
    [Required]
    [RegularExpression("^(Ready|Completed)$")]
    public string Status { get; set; } = string.Empty;
}
