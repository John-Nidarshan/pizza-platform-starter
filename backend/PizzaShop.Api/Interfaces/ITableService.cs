using PizzaShop.Api.Models;

namespace PizzaShop.Api.Interfaces;

public interface ITableService
{
    Task<bool> TableExistsAndActiveAsync(int tableNumber, CancellationToken cancellationToken = default);
}
