using Microsoft.EntityFrameworkCore;
using PizzaShop.Api.Data;
using PizzaShop.Api.Models;
using PizzaShop.Api.Interfaces;

namespace PizzaShop.Api.Services;

public class MenuService : IMenuService
{
    private readonly PizzaShopDbContext _db;

    public MenuService(PizzaShopDbContext db)
    {
        _db = db;
    }

    public async Task<List<MenuItem>> GetAvailableMenuAsync(CancellationToken cancellationToken = default)
    {
        return await _db.MenuItems.AsNoTracking()
            .Where(x => x.IsAvailable)
            .OrderBy(x => x.Category).ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<MenuItem?> GetAvailableMenuItemAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.MenuItems.AsNoTracking()
            .Where(x => x.Id == id && x.IsAvailable)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
