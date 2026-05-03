using DeliveryService.Infrastructure.Data;
using DeliveryService.Infrastructure.Repositories;
using DeliveryService.Infrastructure.Services;
using DeliveryService.Infrastructure.Seed;
using DeliveryService.Core.Interfaces;
using DeliveryService.Core.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<ISenderRepository, SenderRepository>();
builder.Services.AddSingleton<IPackageStatusTransitionValidator, PackageStatusTransitionValidator>();
builder.Services.AddSingleton<IPackageWeightValidator, PackageWeightValidator>();
builder.Services.AddSingleton<ITrackingCodeGenerator, TrackingCodeGenerator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Виклик сідера для наповнення бази даних (тільки якщо база порожня)
// Виклик сідера для наповнення бази даних
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("Checking if seeding needed...");
        var anySenders = await dbContext.Senders.AnyAsync();
        Console.WriteLine($"Any senders: {anySenders}");
        if (!anySenders)
        {
            Console.WriteLine("Seeding database...");
            await DataSeeder.SeedAsync(dbContext);
            Console.WriteLine("Seeding completed.");
        }
        else
        {
            Console.WriteLine("Database already seeded.");
        }
    }
}

app.Run();

public partial class Program { }
