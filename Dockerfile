FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["backend/PizzaShop.Api/PizzaShop.Api.csproj", "backend/PizzaShop.Api/"]
RUN dotnet restore "backend/PizzaShop.Api/PizzaShop.Api.csproj"

COPY backend/PizzaShop.Api/ backend/PizzaShop.Api/
RUN dotnet publish "backend/PizzaShop.Api/PizzaShop.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PizzaShop.Api.dll"]
