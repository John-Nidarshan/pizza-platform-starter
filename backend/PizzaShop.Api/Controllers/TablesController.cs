using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Api.Interfaces;

namespace PizzaShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TablesController(ITableService tables) : ControllerBase
{
    [HttpGet("{tableNumber:int}")]
    public async Task<IActionResult> GetTable(int tableNumber, CancellationToken cancellationToken)
    {
        var exists = await tables.TableExistsAndActiveAsync(tableNumber, cancellationToken);
        return exists ? Ok(new { TableNumber = tableNumber, Active = true }) : NotFound(new { message = "Table not found or inactive." });
    }
}
