using PizzaShop.Api.Models;
using PizzaShop.Api.DTOs;

namespace PizzaShop.Api.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderAsync(Guid id, CancellationToken cancellationToken = default);
}
