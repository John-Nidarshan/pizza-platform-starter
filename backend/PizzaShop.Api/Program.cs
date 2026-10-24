using Microsoft.EntityFrameworkCore;
using PizzaShop.Api.Data;
using PizzaShop.Api.Hubs;
using PizzaShop.Api.Models;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "5080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application services
builder.Services.AddScoped<PizzaShop.Api.Interfaces.IMenuService, PizzaShop.Api.Services.MenuService>();
builder.Services.AddScoped<PizzaShop.Api.Interfaces.ITableService, PizzaShop.Api.Services.TableService>();
builder.Services.AddScoped<PizzaShop.Api.Interfaces.IOrderService, PizzaShop.Api.Services.OrderService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Configure ConnectionStrings:PizzaShop using User Secrets or environment variables.");

builder.Services.AddDbContext<PizzaShopDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddCors(options => options.AddPolicy("AngularFrontend", policy =>
    policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();
app.UseCors("AngularFrontend");
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        // Serve Swagger UI at application root for convenience
        app.UseSwaggerUI(options => options.RoutePrefix = string.Empty);

        // Attempt to open the browser to the app URL (dev convenience)
        try
        {
            var firstUrl = app.Urls.FirstOrDefault() ?? builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5080";
            var openUrl = firstUrl.TrimEnd('/') + "/"; // RoutePrefix = empty so swagger is served at '/'
            var psi = new ProcessStartInfo { FileName = openUrl, UseShellExecute = true };
            Process.Start(psi);
        }
        catch
        {
            // ignore failures to launch browser
        }
    }
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", mode = "sql-server-code-first" }));
app.MapControllers();
app.MapHub<OrderHub>("/hubs/orders");

// Local development convenience: applies checked-in migrations and inserts starter data.
// Generate the first migration with `dotnet ef migrations add InitialCreate` before first run.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PizzaShopDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.MenuItems.AnyAsync())
    {
        db.MenuItems.AddRange(
            new MenuItem { Name = "Pepperoni Party", Description = "Pepperoni, mozzarella, tomato sauce", Category = "Pizza", Price = 12.50m, Emoji = "🍕" },
            new MenuItem { Name = "Classic Margherita", Description = "Tomato, mozzarella, fresh basil", Category = "Pizza", Price = 10.00m, Emoji = "🍅" },
            new MenuItem { Name = "BBQ Chicken", Description = "BBQ sauce, chicken, red onion, mozzarella", Category = "Pizza", Price = 13.50m, Emoji = "🔥" },
            new MenuItem { Name = "Garden Party", Description = "Peppers, mushrooms, olives, red onion", Category = "Pizza", Price = 11.50m, Emoji = "🫑" },
            new MenuItem { Name = "Cheesy Garlic Bread", Description = "Toasted garlic bread with mozzarella", Category = "Sides", Price = 5.00m, Emoji = "🧄" },
            new MenuItem { Name = "Hot Wings", Description = "Six spicy chicken wings", Category = "Sides", Price = 6.50m, Emoji = "🍗" },
            new MenuItem { Name = "Cola", Description = "330ml can", Category = "Drinks", Price = 1.80m, Emoji = "🥤" },
            new MenuItem { Name = "Chocolate Brownie", Description = "Warm chocolate brownie", Category = "Desserts", Price = 4.00m, Emoji = "🍫" });
        await db.SaveChangesAsync();
    }

    if (!await db.RestaurantTables.AnyAsync())
    {
        db.RestaurantTables.AddRange(Enumerable.Range(1, 10)
            .Select(number => new RestaurantTable { TableNumber = number }));
        await db.SaveChangesAsync();
    }
}

app.Run();
