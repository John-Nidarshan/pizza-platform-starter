using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Api.DTOs;
using PizzaShop.Api.Models;
using PizzaShop.Api.Interfaces;

namespace PizzaShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orders) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orders.CreateOrderAsync(request, cancellationToken);
            var isOnline = request.PaymentMethod.Equals("online", StringComparison.OrdinalIgnoreCase);
            var message = isOnline
                ? "Order draft saved. Online checkout is not configured; the order is not paid and must not be prepared."
                : "Order saved awaiting counter payment. Staff must confirm payment before preparation.";

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new
            {
                order.Id, order.TableNumber, order.Total, order.PaymentMethod,
                order.PaymentStatus, order.OrderStatus, order.CreatedAtUtc,
                Message = message,
                Items = order.Items.Select(x => new { x.MenuItemId, x.ItemName, x.UnitPrice, x.Quantity, LineTotal = x.UnitPrice * x.Quantity })
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await orders.GetOrderAsync(id, cancellationToken);
        if (order is null) return NotFound(new { message = "Order not found." });

        return Ok(new
        {
            order.Id, order.TableNumber, order.Total, order.PaymentMethod,
            order.PaymentStatus, order.OrderStatus, order.CreatedAtUtc,
            Items = order.Items.Select(x => new { x.MenuItemId, x.ItemName, x.UnitPrice, x.Quantity, LineTotal = x.UnitPrice * x.Quantity })
        });
    }
}
