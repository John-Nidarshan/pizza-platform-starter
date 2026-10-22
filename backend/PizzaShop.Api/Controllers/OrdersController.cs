using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PizzaShop.Api.DTOs;
using PizzaShop.Api.Hubs;
using PizzaShop.Api.Interfaces;
using PizzaShop.Api.Models;

namespace PizzaShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(
    IOrderService orders,
    IHubContext<OrderHub> orderHub,
    ILogger<OrdersController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await orders.CreateOrderAsync(request, cancellationToken);
            await BroadcastOrderChangedAsync(order, cancellationToken);
            var isOnline = request.PaymentMethod.Equals("online", StringComparison.OrdinalIgnoreCase);
            var message = isOnline
                ? "Order draft saved. Online checkout is not configured; the order is not paid and must not be prepared."
                : "Order saved awaiting counter payment. Staff must confirm payment before preparation.";

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new
            {
                order.Id,
                order.TableNumber,
                order.Total,
                order.PaymentMethod,
                order.PaymentStatus,
                order.OrderStatus,
                order.CreatedAtUtc,
                Message = message,
                Items = order.Items.Select(item => new
                {
                    item.MenuItemId,
                    item.ItemName,
                    item.UnitPrice,
                    item.Quantity,
                    LineTotal = item.UnitPrice * item.Quantity
                })
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        var ordersList = await orders.GetOrdersAsync(cancellationToken);
        return Ok(ordersList.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await orders.GetOrderAsync(id, cancellationToken);
        if (order is null) return NotFound(new { message = "Order not found." });

        return Ok(ToResponse(order));
    }

    [HttpPost("{id:guid}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orders.ConfirmCounterPaymentAsync(id, cancellationToken);
            if (order is null) return NotFound(new { message = "Order not found." });

            await BroadcastOrderChangedAsync(order, cancellationToken);
            return Ok(ToResponse(order));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await orders.UpdateOrderStatusAsync(id, request.Status, cancellationToken);
            if (order is null) return NotFound(new { message = "Order not found." });

            await BroadcastOrderChangedAsync(order, cancellationToken);
            return Ok(ToResponse(order));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private async Task BroadcastOrderChangedAsync(Order order, CancellationToken cancellationToken)
    {
        try
        {
            await orderHub.Clients.All.SendAsync("OrderChanged", ToResponse(order), cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to broadcast order update for {OrderId}.", order.Id);
        }
    }

    private static OrderAdminResponse ToResponse(Order order) => new()
    {
        Id = order.Id,
        TableNumber = order.TableNumber,
        Total = order.Total,
        PaymentMethod = order.PaymentMethod,
        PaymentStatus = order.PaymentStatus,
        OrderStatus = order.OrderStatus,
        CreatedAtUtc = order.CreatedAtUtc,
        Items = order.Items.Select(item => new OrderAdminItemResponse
        {
            MenuItemId = item.MenuItemId,
            ItemName = item.ItemName,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            LineTotal = item.UnitPrice * item.Quantity
        }).ToList()
    };
}
