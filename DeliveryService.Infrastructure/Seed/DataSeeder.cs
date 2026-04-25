using DeliveryService.Core.Entities;
using DeliveryService.Core.Enums;
using DeliveryService.Infrastructure.Data;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Infrastructure.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context, int senderCount = 200, int packagesPerSender = 50)
    {
        if (await context.Senders.AnyAsync())
            return;

        // 1. Відправники
        var senderFaker = new Faker<Sender>()
            .RuleFor(s => s.Id, f => Guid.NewGuid())
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
            for (int i = 0; i < packagesPerSender; i++)
            {
                var f = new Faker();

                string trackingCode;
                do
                {
                    trackingCode = GenerateTrackingCode(f);
                } while (!usedTrackingCodes.Add(trackingCode));

                var status = f.PickRandom<PackageStatus>();

                var package = new Package
                {
                    Id = Guid.NewGuid(),
                    SenderId = sender.Id,
                    RecipientName = f.Person.FullName,
                    RecipientAddress = f.Address.FullAddress(),
                    RecipientPhone = f.Person.Phone,
                    Weight = Math.Round(f.Random.Decimal(0.1m, 50m), 2),
                    Dimensions = $"{f.Random.Int(10, 50)}x{f.Random.Int(10, 50)}x{f.Random.Int(5, 30)}",
                    Status = status,
                    TrackingCode = trackingCode,
                    CreatedAt = DateTime.SpecifyKind(f.Date.Past(1), DateTimeKind.Utc)
                };
                packages.Add(package);

                var statusSequence = Enum.GetValues<PackageStatus>()
                    .Where(s => s <= status)
                    .ToList();

                var timestamp = package.CreatedAt;
                foreach (var st in statusSequence)
                {
                    timestamp = timestamp.AddMinutes(f.Random.Int(1, 4320));
                    // явно перетворюємо в UTC, якщо раптом не UTC
                    if (timestamp.Kind != DateTimeKind.Utc)
                        timestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
                    updates.Add(new DeliveryUpdate
                    {
                        Id = Guid.NewGuid(),
                        PackageId = package.Id,
                        Status = st,
                        Location = f.Address.City(),
                        Notes = st == PackageStatus.Delivered ? "Delivered successfully" : f.Lorem.Sentence(),
                        Timestamp = timestamp,
                        UpdatedBy = f.PickRandom("system", "warehouse", "courier_" + f.Random.Int(1, 50))
                    });
                }

                // Зберігаємо пакунками по 500 записів, щоб зменшити навантаження на пам'ять
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

        if (packages.Any())
        {
            await context.Packages.AddRangeAsync(packages);
            await context.DeliveryUpdates.AddRangeAsync(updates);
            await context.SaveChangesAsync();
        }
    }

    private static string GenerateTrackingCode(Faker faker)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[faker.Random.Int(0, s.Length - 1)])
            .ToArray());
    }
}