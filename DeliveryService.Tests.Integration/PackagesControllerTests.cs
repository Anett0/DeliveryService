using System.Net;
using System.Net.Http.Json;
using DeliveryService.API.DTOs;
using DeliveryService.Core.Entities;
using DeliveryService.Infrastructure.Data;
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
        var senderId = await AddSenderAsync();
        var createDto = new CreatePackageDto
        {
            SenderId = senderId,
            RecipientName = "John Doe",
            RecipientAddress = "123 Main St",
            RecipientPhone = "555-1234",
            Weight = 5.5M,
            Dimensions = "30x20x10"
        };

        var response = await _client.PostAsJsonAsync("/api/packages", createDto);
        var package = await response.Content.ReadFromJsonAsync<PackageResponseDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(package);
        Assert.NotEmpty(package.TrackingCode);
        Assert.Equal("Created", package.Status);
    }

    [Fact]
    public async Task CreatePackage_WithInvalidDto_ShouldReturnBadRequest()
    {
        var createDto = new CreatePackageDto
        {
            SenderId = Guid.Empty,
            RecipientName = " ",
            RecipientAddress = "123 Main St",
            RecipientPhone = "555-1234",
            Weight = 60M,
            Dimensions = "30x20x10"
        };

        var response = await _client.PostAsJsonAsync("/api/packages", createDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePackage_WithMissingSender_ShouldReturnNotFound()
    {
        var createDto = new CreatePackageDto
        {
            SenderId = Guid.NewGuid(),
            RecipientName = "John Doe",
            RecipientAddress = "123 Main St",
            RecipientPhone = "555-1234",
            Weight = 5.5M,
            Dimensions = "30x20x10"
        };

        var response = await _client.PostAsJsonAsync("/api/packages", createDto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TrackPackage_WithUnknownTrackingCode_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/packages/UNKNOWN12345");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByStatus_WithInvalidStatus_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync("/api/packages?status=Unknown");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddUpdate_WithInvalidStatus_ShouldReturnBadRequest()
    {
        var package = await CreatePackageAsync();
        var updateDto = new UpdateStatusDto
        {
            Status = "Unknown",
            Location = "Warehouse",
            Notes = "Invalid status test",
            UpdatedBy = "system"
        };

        var response = await _client.PostAsJsonAsync($"/api/packages/{package.Id}/update", updateDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddUpdate_WithMissingPackage_ShouldReturnNotFound()
    {
        var updateDto = new UpdateStatusDto
        {
            Status = "PickedUp",
            Location = "Warehouse",
            Notes = "Missing package test",
            UpdatedBy = "system"
        };

        var response = await _client.PostAsJsonAsync($"/api/packages/{Guid.NewGuid()}/update", updateDto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PackageLifecycle_ShouldMoveFromCreatedToDelivered()
    {
        var package = await CreatePackageAsync();

        await UpdateStatusAsync(package.Id, "PickedUp");
        await UpdateStatusAsync(package.Id, "InTransit");
        await UpdateStatusAsync(package.Id, "OutForDelivery");

        var deliveredResponse = await _client.PatchAsync($"/api/packages/{package.Id}/deliver", null);
        Assert.Equal(HttpStatusCode.OK, deliveredResponse.StatusCode);

        var trackedPackage = await GetPackageAsync(package.TrackingCode);
        Assert.Equal("Delivered", trackedPackage.Status);
    }

    [Fact]
    public async Task TrackPackage_WithExistingTrackingCode_ShouldReturnPackage()
    {
        var package = await CreatePackageAsync();

        var response = await _client.GetAsync($"/api/packages/{package.TrackingCode}");
        var trackedPackage = await response.Content.ReadFromJsonAsync<PackageResponseDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(trackedPackage);
        Assert.Equal(package.Id, trackedPackage.Id);
        Assert.Equal(package.TrackingCode, trackedPackage.TrackingCode);
    }

    [Fact]
    public async Task GetUpdates_ShouldReturnChronologicalStatusHistory()
    {
        var package = await CreatePackageAsync();
        await UpdateStatusAsync(package.Id, "PickedUp");
        await UpdateStatusAsync(package.Id, "InTransit");
        await UpdateStatusAsync(package.Id, "OutForDelivery");

        var response = await _client.GetAsync($"/api/packages/{package.TrackingCode}/updates");
        var updates = await response.Content.ReadFromJsonAsync<List<DeliveryUpdateDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updates);
        Assert.Equal(new[] { "Created", "PickedUp", "InTransit", "OutForDelivery" }, updates.Select(u => u.Status));
        Assert.Equal(updates.OrderBy(u => u.Timestamp).Select(u => u.Timestamp), updates.Select(u => u.Timestamp));
    }

    [Fact]
    public async Task AddUpdate_WithInvalidTransition_ShouldReturnBadRequest()
    {
        var package = await CreatePackageAsync();
        var response = await UpdateStatusAsync(package.Id, "InTransit", ensureSuccess: false);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddUpdate_WithDeliveredPackage_ShouldReturnBadRequest()
    {
        var package = await CreatePackageAsync();
        await UpdateStatusAsync(package.Id, "PickedUp");
        await UpdateStatusAsync(package.Id, "InTransit");
        await UpdateStatusAsync(package.Id, "OutForDelivery");

        var deliveredResponse = await _client.PatchAsync($"/api/packages/{package.Id}/deliver", null);
        Assert.Equal(HttpStatusCode.OK, deliveredResponse.StatusCode);

        var response = await UpdateStatusAsync(package.Id, "Returned", ensureSuccess: false);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddUpdate_WithReturnedPackage_ShouldReturnBadRequest()
    {
        var package = await CreatePackageAsync();

        var returnResponse = await _client.PatchAsync($"/api/packages/{package.Id}/return", null);
        Assert.Equal(HttpStatusCode.OK, returnResponse.StatusCode);

        var response = await UpdateStatusAsync(package.Id, "PickedUp", ensureSuccess: false);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByStatus_ShouldReturnPackagesWithRequestedStatus()
    {
        var package = await CreatePackageAsync();
        await UpdateStatusAsync(package.Id, "PickedUp");

        var response = await _client.GetAsync("/api/packages?status=PickedUp");
        var packages = await response.Content.ReadFromJsonAsync<List<PackageResponseDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(packages);
        Assert.Contains(packages, p => p.Id == package.Id);
        Assert.All(packages, p => Assert.Equal("PickedUp", p.Status));
    }

    [Fact]
    public async Task GetSenderPackages_ShouldReturnPackagesForSender()
    {
        var senderId = await AddSenderAsync();
        var firstPackage = await CreatePackageAsync(senderId);
        var secondPackage = await CreatePackageAsync(senderId);

        var response = await _client.GetAsync($"/api/senders/{senderId}/packages");
        var packages = await response.Content.ReadFromJsonAsync<List<PackageResponseDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(packages);
        Assert.Contains(packages, p => p.Id == firstPackage.Id);
        Assert.Contains(packages, p => p.Id == secondPackage.Id);
        Assert.All(packages, p => Assert.Equal(senderId, p.SenderId));
    }

    private async Task<PackageResponseDto> CreatePackageAsync(Guid? senderId = null)
    {
        var packageSenderId = senderId ?? await AddSenderAsync();
        var createDto = new CreatePackageDto
        {
            SenderId = packageSenderId,
            RecipientName = "John Doe",
            RecipientAddress = "123 Main St",
            RecipientPhone = "555-1234",
            Weight = 5.5M,
            Dimensions = "30x20x10"
        };

        var response = await _client.PostAsJsonAsync("/api/packages", createDto);
        response.EnsureSuccessStatusCode();

        var package = await response.Content.ReadFromJsonAsync<PackageResponseDto>();
        Assert.NotNull(package);

        return package;
    }

    private async Task<PackageResponseDto> GetPackageAsync(string trackingCode)
    {
        var response = await _client.GetAsync($"/api/packages/{trackingCode}");
        response.EnsureSuccessStatusCode();

        var package = await response.Content.ReadFromJsonAsync<PackageResponseDto>();
        Assert.NotNull(package);

        return package;
    }

    private async Task<HttpResponseMessage> UpdateStatusAsync(
        Guid packageId,
        string status,
        bool ensureSuccess = true)
    {
        var updateDto = new UpdateStatusDto
        {
            Status = status,
            Location = $"{status} location",
            Notes = $"{status} notes",
            UpdatedBy = "integration-test"
        };

        var response = await _client.PostAsJsonAsync($"/api/packages/{packageId}/update", updateDto);

        if (ensureSuccess)
            response.EnsureSuccessStatusCode();

        return response;
    }

    private async Task<Guid> AddSenderAsync()
    {
        var senderId = Guid.NewGuid();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Senders.Add(new Sender
        {
            Id = senderId,
            Name = "Test Sender",
            Email = $"test-{senderId:N}@example.com",
            Phone = "123456789",
            Address = "Test Address"
        });
        await db.SaveChangesAsync();

        return senderId;
    }
}
