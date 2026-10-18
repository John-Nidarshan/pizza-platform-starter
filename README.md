# Slice Club — Pizza Shop QR Ordering Project

A full starter source project for a dine-in pizza shop ordering experience.

## Included

- Angular 19 customer menu, category filters, basket, quantities, table number from `?table=7`, and online/counter payment choice.
- ASP.NET Core 8 Web API using Controllers.
- EF Core Code First + SQL Server entities, DbContext and migration workflow.
- Controllers for menu, table lookup, and order creation/retrieval.
- Development seed data for 8 menu items and 10 tables.
- Swagger in Development.

## Important limitations

This is a development starter, not a production-ready restaurant POS or payment system. It does not collect real payments. An `online` order remains unpaid until a real payment provider and signed webhook are implemented. A `counter` order remains unpaid until authorised staff confirm payment. No kitchen fulfilment dashboard, staff authentication, or Uber Eats/Deliveroo API integration is included yet.

Do not accept live customer orders or money until payment confirmation, staff roles, secure table tokens, rate limiting, monitoring, backups, privacy notices, tax/receipt settings and operational procedures are implemented and tested.

## Requirements

- .NET 8 SDK
- SQL Server Express or another SQL Server instance
- Node.js 20.11+ recommended for Angular 19
- npm

## 1. Configure SQL Server

In PowerShell, go to the API folder:

```powershell
cd backend/PizzaShop.Api
dotnet restore
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:PizzaShop" "Server=localhost\SQLEXPRESS;Database=PizzaShopDb;Trusted_Connection=True;TrustServerCertificate=True"
```

Replace `localhost\SQLEXPRESS` with your actual SQL Server instance. For LocalDB, use `Server=(localdb)\MSSQLLocalDB;Database=PizzaShopDb;Trusted_Connection=True;TrustServerCertificate=True`.

The trust-server-certificate option is for local development only. Configure secure certificate validation and secret storage for production.

## 2. Create the Code First migration

Run once from `backend/PizzaShop.Api`:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.0
dotnet ef migrations add InitialCreate
dotnet ef database update
```

If `dotnet-ef` is already installed, skip its install command. Commit the generated `Migrations` folder to source control. Do not create the tables manually with SQL. The first migration creates them from the C# model. In Development, the API applies migrations and seeds sample menu/table records on startup.

For future model changes, edit the C# entity/configuration, then run `dotnet ef migrations add DescribeYourChange` and `dotnet ef database update`.

## 3. Run the backend

From `backend/PizzaShop.Api`:

```powershell
dotnet run
```

The API listens at `http://localhost:5080`. Development Swagger is at `http://localhost:5080/swagger`.

Useful endpoints:

- `GET /api/health`
- `GET /api/menu`
- `GET /api/menu/1`
- `GET /api/tables/1`
- `POST /api/orders`
- `GET /api/orders/{guid}`

Example order body:

```json
{
  "tableNumber": 1,
  "paymentMethod": "counter",
  "items": [
    { "menuItemId": 1, "quantity": 2 },
    { "menuItemId": 5, "quantity": 1 }
  ]
}
```

The API uses numeric database IDs, calculates totals on the server and stores item-name/price snapshots. It does not accept prices from the browser.

## 4. Run Angular

In a second terminal:

```powershell
cd frontend
npm install
npm start
```

Open `http://localhost:4200`. The Angular dev-server proxy forwards `/api` requests to `http://localhost:5080`.

Scan or test a table URL by opening `http://localhost:4200/?table=1`. The numeric table query is only a starter convenience; for a public deployment, use unguessable revocable QR table tokens and validate them on the server.

## Next steps before launch

1. Add real online checkout with server-created payment sessions and verified, idempotent signed webhooks.
2. Add staff login/roles and a secure order-status workflow; only verified paid/authorised orders should enter the kitchen queue.
3. Add table tokens, order-abuse protections, stock controls, receipts, refunds, tax settings, audit logs, backups and monitoring.
4. Build a kitchen/staff dashboard and live updates (e.g. SignalR).
5. Investigate approved Uber Eats and Deliveroo POS/API integration after the core ordering flow is stable. Platform integrations require provider access/approval and are not included in this starter.
