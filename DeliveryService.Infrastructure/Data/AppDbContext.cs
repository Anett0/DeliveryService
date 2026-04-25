using DeliveryService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Sender> Senders => Set<Sender>();
    public DbSet<DeliveryUpdate> DeliveryUpdates => Set<DeliveryUpdate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Package>()
            .HasIndex(p => p.TrackingCode)
            .IsUnique();

        modelBuilder.Entity<Package>()
            .Property(p => p.Weight)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Package>()
            .HasOne(p => p.Sender)
            .WithMany(s => s.Packages)
            .HasForeignKey(p => p.SenderId);

        modelBuilder.Entity<DeliveryUpdate>()
            .HasOne(u => u.Package)
            .WithMany(p => p.Updates)
            .HasForeignKey(u => u.PackageId);
    }
}