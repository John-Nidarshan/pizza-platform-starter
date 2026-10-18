namespace PizzaShop.Api.Models;

public class RestaurantTable
{
    public int Id { get; set; }
    public int TableNumber { get; set; }
    public bool IsActive { get; set; } = true;
}
