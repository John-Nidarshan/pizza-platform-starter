using Microsoft.AspNetCore.Mvc;
using PizzaShop.Api.Interfaces;

namespace PizzaShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController(IMenuService menuService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMenu(CancellationToken cancellationToken)
    {
        var items = await menuService.GetAvailableMenuAsync(cancellationToken);
        var projected = items.Select(x => new { x.Id, x.Name, x.Description, x.Category, x.Price, x.ImageUrl, x.Emoji, Available = x.IsAvailable });
        return Ok(projected);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMenuItem(int id, CancellationToken cancellationToken)
    {
        var item = await menuService.GetAvailableMenuItemAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(new { item.Id, item.Name, item.Description, item.Category, item.Price, item.ImageUrl, item.Emoji, Available = item.IsAvailable });
    }
}
