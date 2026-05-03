using Bogus;
using DeliveryService.Core.Entities;
using DeliveryService.Core.Enums;
using DeliveryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Infrastructure.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context, int senderCount = 200, int packagesPerSender = 50)
    {
        if (await context.Senders.AnyAsync())
        {
            // Add test package
            var existing = await context.Packages.FirstOrDefaultAsync(p => p.TrackingCode == "TEST123");
            if (existing != null)
            {
                context.Packages.Remove(existing);
                var existingUpdates = context.DeliveryUpdates.Where(u => u.PackageId == existing.Id);
                context.DeliveryUpdates.RemoveRange(existingUpdates);
                await context.SaveChangesAsync();
            }
            var testSender2 = await context.Senders.FirstAsync();
            var testPackage2 = new Package
            {
                Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                SenderId = testSender2.Id,
                RecipientName = "Test Recipient",
                RecipientAddress = "Test Address",
                RecipientPhone = "123-456-7890",
                Weight = 1.0m,
                Dimensions = "10x10x10",
                Status = PackageStatus.Created,
                TrackingCode = "TEST123",
                CreatedAt = DateTime.UtcNow
            };
            await context.Packages.AddAsync(testPackage2);
            var testUpdate2 = new DeliveryUpdate
            {
                Id = Guid.NewGuid(),
                PackageId = testPackage2.Id,
                Status = PackageStatus.Created,
                Location = "Warehouse",
                Notes = "Test package created",
                Timestamp = DateTime.UtcNow,
                UpdatedBy = "system"
            };
            await context.DeliveryUpdates.AddAsync(testUpdate2);
            await context.SaveChangesAsync();
            return;
        }

        var senderFaker = new Faker<Sender>()
            .RuleFor(s => s.Id, _ => Guid.NewGuid())
            .RuleFor(s => s.Name, f => f.Person.FullName)
            .RuleFor(s => s.Email, f => f.Person.Email)
            .RuleFor(s => s.Phone, f => f.Person.Phone)
            .RuleFor(s => s.Address, f => f.Address.FullAddress());

        var senders = senderFaker.Generate(senderCount);
        await context.Senders.AddRangeAsync(senders);
        await context.SaveChangesAsync();

        var packages = new List<Package>();
        var updates = new List<DeliveryUpdate>();
        var usedTrackingCodes = new HashSet<string>();

        foreach (var sender in senders)
        {
            for (var i = 0; i < packagesPerSender; i++)
            {
                var faker = new Faker();

                string trackingCode;
                do
                {
                    trackingCode = GenerateTrackingCode(faker);
                } while (!usedTrackingCodes.Add(trackingCode));

                var status = faker.PickRandom<PackageStatus>();
                var package = new Package
                {
                    Id = Guid.NewGuid(),
                    SenderId = sender.Id,
                    RecipientName = faker.Person.FullName,
                    RecipientAddress = faker.Address.FullAddress(),
                    RecipientPhone = faker.Person.Phone,
                    Weight = Math.Round(faker.Random.Decimal(0.1m, 50m), 2),
                    Dimensions = $"{faker.Random.Int(10, 50)}x{faker.Random.Int(10, 50)}x{faker.Random.Int(5, 30)}",
                    Status = status,
                    TrackingCode = trackingCode,
                    CreatedAt = DateTime.SpecifyKind(faker.Date.Past(1), DateTimeKind.Utc)
                };

                packages.Add(package);

                var timestamp = package.CreatedAt;
                foreach (var updateStatus in BuildStatusHistory(status, faker))
                {
                    timestamp = DateTime.SpecifyKind(
                        timestamp.AddMinutes(faker.Random.Int(1, 4320)),
                        DateTimeKind.Utc);

                    updates.Add(new DeliveryUpdate
                    {
                        Id = Guid.NewGuid(),
                        PackageId = package.Id,
                        Status = updateStatus,
                        Location = faker.Address.City(),
                        Notes = updateStatus switch
                        {
                            PackageStatus.Delivered => "Delivered successfully",
                            PackageStatus.Returned => "Returned to sender",
                            _ => faker.Lorem.Sentence()
                        },
                        Timestamp = timestamp,
                        UpdatedBy = faker.PickRandom("system", "warehouse", "courier_" + faker.Random.Int(1, 50))
                    });
                }

                if (packages.Count >= 500)
                {
                    await context.Packages.AddRangeAsync(packages);
                    await context.DeliveryUpdates.AddRangeAsync(updates);
                    await context.SaveChangesAsync();
                    packages.Clear();
                    updates.Clear();
                }
            }
        }

        // Add a test package with known tracking code for k6 tests
        var testSender = await context.Senders.FirstAsync();
        var testPackageId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var testPackage = new Package
        {
            Id = testPackageId,
            SenderId = testSender.Id,
            RecipientName = "Test Recipient",
            RecipientAddress = "Test Address",
            RecipientPhone = "123-456-7890",
            Weight = 1.0m,
            Dimensions = "10x10x10",
            Status = PackageStatus.Created,
            TrackingCode = "TEST123",
            CreatedAt = DateTime.UtcNow
        };
        await context.Packages.AddAsync(testPackage);
        var testUpdate = new DeliveryUpdate
        {
            Id = Guid.NewGuid(),
            PackageId = testPackage.Id,
            Status = PackageStatus.Created,
            Location = "Warehouse",
            Notes = "Test package created",
            Timestamp = DateTime.UtcNow,
            UpdatedBy = "system"
        };
        await context.DeliveryUpdates.AddAsync(testUpdate);
        await context.SaveChangesAsync();
    }

    private static IReadOnlyList<PackageStatus> BuildStatusHistory(PackageStatus finalStatus, Faker faker)
    {
        var deliveryFlow = new[]
        {
            PackageStatus.Created,
            PackageStatus.PickedUp,
            PackageStatus.InTransit,
            PackageStatus.OutForDelivery,
            PackageStatus.Delivered
        };

        if (finalStatus != PackageStatus.Returned)
            return deliveryFlow.TakeWhile(status => status <= finalStatus).ToList();

        var returnFrom = faker.PickRandom(
            PackageStatus.Created,
            PackageStatus.PickedUp,
            PackageStatus.InTransit,
            PackageStatus.OutForDelivery);

        return deliveryFlow
            .TakeWhile(status => status <= returnFrom)
            .Append(PackageStatus.Returned)
            .ToList();
    }

    private static string GenerateTrackingCode(Faker faker)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[faker.Random.Int(0, s.Length - 1)])
            .ToArray());
    }
}
