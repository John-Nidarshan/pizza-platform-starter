FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PizzaShop.Api/PizzaShop.Api.csproj", "PizzaShop.Api/"]
RUN dotnet restore "PizzaShop.Api/PizzaShop.Api.csproj"

COPY PizzaShop.Api/ backend/PizzaShop.Api/
RUN dotnet publish "PizzaShop.Api/PizzaShop.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PizzaShop.Api.dll"]
