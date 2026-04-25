using System.Net.Http.Json;
using DeliveryService.API.DTOs;
using DeliveryService.Core.Entities;
using DeliveryService.Infrastructure.Data;   // <-- додано для AppDbContext
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DeliveryService.Tests.Integration;

public class PackagesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public PackagesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreatePackage_ShouldReturnTrackingCode()
    {
        // Arrange: створити відправника
        var senderId = Guid.NewGuid();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Senders.Add(new Sender
            {
                Id = senderId,
                Name = "Test Sender",
                Email = "test@example.com",
                Phone = "123456789",
                Address = "Test Address"
            });
            await db.SaveChangesAsync();
        }

        var createDto = new CreatePackageDto
        {
            SenderId = senderId,
            RecipientName = "John Doe",
            RecipientAddress = "123 Main St",
            RecipientPhone = "555-1234",
            Weight = 5.5M,   // <-- додано суфікс M
            Dimensions = "30x20x10"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/packages", createDto);
        var package = await response.Content.ReadFromJsonAsync<PackageResponseDto>();

        // Assert
        Assert.NotNull(package);
        Assert.NotEmpty(package.TrackingCode);
        Assert.Equal("Created", package.Status);
    }
}