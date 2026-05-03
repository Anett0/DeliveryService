using DeliveryService.Core.Entities;
using DeliveryService.Core.Enums;
using DeliveryService.Infrastructure.Data;
using DeliveryService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DeliveryService.Tests.Integration;

public class DatabaseBehaviorTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DatabaseBehaviorTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Packages_ShouldEnforceUniqueTrackingCode()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sender = CreateSender();
        const string trackingCode = "ABC123456789";

        db.Senders.Add(sender);
        db.Packages.Add(CreatePackage(sender.Id, trackingCode));
        db.Packages.Add(CreatePackage(sender.Id, trackingCode));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task UpdatePackageStatusAsync_ShouldPersistDeliveryUpdateAuditRecord()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var repository = new PackageRepository(db);
        var sender = CreateSender();
        var package = CreatePackage(sender.Id, "AUDIT1234567");

        db.Senders.Add(sender);
        db.Packages.Add(package);
        await db.SaveChangesAsync();

        package.Status = PackageStatus.PickedUp;
        var update = new DeliveryUpdate
        {
            Id = Guid.NewGuid(),
            PackageId = package.Id,
            Status = PackageStatus.PickedUp,
            Location = "Warehouse",
            Notes = "Picked up by courier",
            Timestamp = DateTime.UtcNow,
            UpdatedBy = "integration-test"
        };

        await repository.UpdatePackageStatusAsync(package, update);

        var savedPackage = await db.Packages.SingleAsync(p => p.Id == package.Id);
        var savedUpdates = await db.DeliveryUpdates
            .Where(u => u.PackageId == package.Id)
            .ToListAsync();

        Assert.Equal(PackageStatus.PickedUp, savedPackage.Status);
        var savedUpdate = Assert.Single(savedUpdates);
        Assert.Equal(PackageStatus.PickedUp, savedUpdate.Status);
        Assert.Equal("integration-test", savedUpdate.UpdatedBy);
    }

    [Fact]
    public async Task Sender_ShouldLoadRelatedPackages()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sender = CreateSender();
        var firstPackage = CreatePackage(sender.Id, "RELATION1234");
        var secondPackage = CreatePackage(sender.Id, "RELATION5678");

        db.Senders.Add(sender);
        db.Packages.AddRange(firstPackage, secondPackage);
        await db.SaveChangesAsync();

        var savedSender = await db.Senders
            .Include(s => s.Packages)
            .SingleAsync(s => s.Id == sender.Id);

        Assert.Equal(2, savedSender.Packages.Count);
        Assert.Contains(savedSender.Packages, p => p.Id == firstPackage.Id);
        Assert.Contains(savedSender.Packages, p => p.Id == secondPackage.Id);
        Assert.All(savedSender.Packages, p => Assert.Equal(sender.Id, p.SenderId));
    }

    private static Sender CreateSender() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Database Test Sender",
        Email = $"db-test-{Guid.NewGuid():N}@example.com",
        Phone = "123456789",
        Address = "Database Test Address"
    };

    private static Package CreatePackage(Guid senderId, string trackingCode) => new()
    {
        Id = Guid.NewGuid(),
        SenderId = senderId,
        RecipientName = "Database Test Recipient",
        RecipientAddress = "Database Test Recipient Address",
        RecipientPhone = "555-1234",
        Weight = 5.5M,
        Dimensions = "30x20x10",
        Status = PackageStatus.Created,
        TrackingCode = trackingCode,
        CreatedAt = DateTime.UtcNow
    };
}
