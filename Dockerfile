# Use the official .NET 8.0 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["DeliveryService.API/DeliveryService.API.csproj", "DeliveryService.API/"]
COPY ["DeliveryService.Core/DeliveryService.Core.csproj", "DeliveryService.Core/"]
COPY ["DeliveryService.Infrastructure/DeliveryService.Infrastructure.csproj", "DeliveryService.Infrastructure/"]
RUN dotnet restore "DeliveryService.API/DeliveryService.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/DeliveryService.API"
RUN dotnet build "DeliveryService.API.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "DeliveryService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 8.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DeliveryService.API.dll"]