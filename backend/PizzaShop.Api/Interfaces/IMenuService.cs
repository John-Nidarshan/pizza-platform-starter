using PizzaShop.Api.Models;

namespace PizzaShop.Api.Interfaces;

public interface IMenuService
{
    Task<List<MenuItem>> GetAvailableMenuAsync(CancellationToken cancellationToken = default);
    Task<MenuItem?> GetAvailableMenuItemAsync(int id, CancellationToken cancellationToken = default);
}
