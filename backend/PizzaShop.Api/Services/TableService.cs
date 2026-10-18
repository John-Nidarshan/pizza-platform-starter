using Microsoft.EntityFrameworkCore;
using PizzaShop.Api.Data;
using PizzaShop.Api.Interfaces;

namespace PizzaShop.Api.Services;

public class TableService : ITableService
{
    private readonly PizzaShopDbContext _db;

    public TableService(PizzaShopDbContext db)
    {
        _db = db;
    }

    public async Task<bool> TableExistsAndActiveAsync(int tableNumber, CancellationToken cancellationToken = default)
    {
        return await _db.RestaurantTables.AsNoTracking()
            .AnyAsync(x => x.TableNumber == tableNumber && x.IsActive, cancellationToken);
    }
}
