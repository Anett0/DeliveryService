using DeliveryService.Core.Enums;
using DeliveryService.Infrastructure.Data;
using DeliveryService.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DeliveryService.Tests.Integration;

public class DataSeederTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DataSeederTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SeedAsync_WithDefaultOptions_ShouldCreateAtLeastTenThousandPackages()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await EnsureSeededAsync(db);

        Assert.True(await db.Senders.CountAsync() >= 200);
        Assert.True(await db.Packages.CountAsync() >= 10_000);
        Assert.True(await db.DeliveryUpdates.CountAsync() >= 10_000);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateValidSenderPackageUpdateRelationships()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await EnsureSeededAsync(db);

        var packagesWithoutSender = await db.Packages
            .CountAsync(package => !db.Senders.Any(sender => sender.Id == package.SenderId));
        var packagesWithoutUpdates = await db.Packages
            .CountAsync(package => !db.DeliveryUpdates.Any(update => update.PackageId == package.Id));
        var updatesWithoutPackage = await db.DeliveryUpdates
            .CountAsync(update => !db.Packages.Any(package => package.Id == update.PackageId));

        Assert.Equal(0, packagesWithoutSender);
        Assert.Equal(0, packagesWithoutUpdates);
        Assert.Equal(0, updatesWithoutPackage);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateReturnedHistoriesWithoutDeliveredStatus()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await EnsureSeededAsync(db);

        var returnedPackages = await db.Packages
            .Where(package => package.Status == PackageStatus.Returned)
            .Select(package => package.Id)
            .ToListAsync();

        Assert.NotEmpty(returnedPackages);

        var returnedUpdates = await db.DeliveryUpdates
            .Where(update => returnedPackages.Contains(update.PackageId))
            .GroupBy(update => update.PackageId)
            .Select(group => new
            {
                HasDelivered = group.Any(update => update.Status == PackageStatus.Delivered),
                LastStatus = group
                    .OrderBy(update => update.Timestamp)
                    .Select(update => update.Status)
                    .Last()
            })
            .ToListAsync();

        Assert.All(returnedUpdates, history =>
        {
            Assert.False(history.HasDelivered);
            Assert.Equal(PackageStatus.Returned, history.LastStatus);
        });
    }

    private static async Task EnsureSeededAsync(AppDbContext db)
    {
        await DataSeeder.SeedAsync(db);
    }
}
