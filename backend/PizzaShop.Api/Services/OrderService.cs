using Microsoft.EntityFrameworkCore;
using PizzaShop.Api.Data;
using PizzaShop.Api.DTOs;
using PizzaShop.Api.Models;
using PizzaShop.Api.Interfaces;

namespace PizzaShop.Api.Services;

public class OrderService : IOrderService
{
    private readonly PizzaShopDbContext _db;
    private readonly ITableService _tableService;

    public OrderService(PizzaShopDbContext db, ITableService tableService)
    {
        _db = db;
        _tableService = tableService;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var tableExists = await _tableService.TableExistsAndActiveAsync(request.TableNumber, cancellationToken);
        if (!tableExists)
            throw new InvalidOperationException("The selected table does not exist or is inactive.");
        if (request.Items is null || request.Items.Count == 0)
            throw new InvalidOperationException("Your order must contain at least one item.");

        var requestedItems = request.Items.GroupBy(x => x.MenuItemId)
            .Select(group => new { MenuItemId = group.Key, Quantity = group.Sum(x => x.Quantity) })
            .ToList();
        if (requestedItems.Any(x => x.MenuItemId <= 0 || x.Quantity is < 1 or > 20))
            throw new InvalidOperationException("Each item must have a valid ID and quantity between 1 and 20.");

        var ids = requestedItems.Select(x => x.MenuItemId).ToList();
        var menuItems = await _db.MenuItems
            .Where(x => ids.Contains(x.Id) && x.IsAvailable)
            .ToListAsync(cancellationToken);
        if (menuItems.Count != ids.Count)
            throw new InvalidOperationException("One or more menu items do not exist or are unavailable.");

        var isOnline = request.PaymentMethod.Equals("online", StringComparison.OrdinalIgnoreCase);
        var order = new Order
        {
            TableNumber = request.TableNumber,
            PaymentMethod = isOnline ? "online" : "counter",
            PaymentStatus = "Unpaid",
            OrderStatus = isOnline ? "AwaitingPayment" : "AwaitingCounterPayment",
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var requested in requestedItems)
        {
            var menuItem = menuItems.Single(x => x.Id == requested.MenuItemId);
            order.Items.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                ItemName = menuItem.Name,
                UnitPrice = menuItem.Price,
                Quantity = requested.Quantity
            });
        }

        order.Total = order.Items.Sum(x => x.UnitPrice * x.Quantity);
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        return order;
    }

    public async Task<Order?> GetOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Orders.AsNoTracking().Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Orders.AsNoTracking()
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> ConfirmCounterPaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null)
            return null;
        if (!string.Equals(order.PaymentMethod, "counter", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only counter-payment orders can be manually confirmed.");
        if (order.PaymentStatus != "Unpaid" || order.OrderStatus != "AwaitingCounterPayment")
            throw new InvalidOperationException("This order is not awaiting counter payment.");

        order.PaymentStatus = "Paid";
        order.OrderStatus = "Preparing";
        order.Payments.Add(new Payment
        {
            Provider = "Counter",
            Amount = order.Total,
            Status = "Paid",
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);

        return order;
    }

    public async Task<Order?> UpdateOrderStatusAsync(
        Guid id,
        string status,
        CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null)
            return null;
        if (order.PaymentStatus != "Paid")
            throw new InvalidOperationException("Payment must be confirmed before preparing an order.");

        var validTransition =
            (order.OrderStatus == "Preparing" && status == "Ready") ||
            (order.OrderStatus == "Ready" && status == "Completed");
        if (!validTransition)
            throw new InvalidOperationException($"Order status cannot change from {order.OrderStatus} to {status}.");

        order.OrderStatus = status;
        await _db.SaveChangesAsync(cancellationToken);

        return order;
    }
}
