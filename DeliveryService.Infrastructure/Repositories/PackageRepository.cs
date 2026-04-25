using DeliveryService.Core.Entities;
using DeliveryService.Core.Enums;
using DeliveryService.Core.Interfaces;
using DeliveryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Infrastructure.Repositories;

public class PackageRepository : IPackageRepository
{
    private readonly AppDbContext _context;

    public PackageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Package?> GetByIdAsync(Guid id)
        => await _context.Packages.Include(p => p.Updates).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Package?> GetByTrackingCodeAsync(string trackingCode)
        => await _context.Packages.Include(p => p.Updates).FirstOrDefaultAsync(p => p.TrackingCode == trackingCode);

    public async Task<IEnumerable<Package>> GetAllAsync()
        => await _context.Packages.Include(p => p.Updates).ToListAsync();

    public async Task<IEnumerable<Package>> GetByStatusAsync(PackageStatus status)
        => await _context.Packages.Where(p => p.Status == status).Include(p => p.Updates).ToListAsync();

    public async Task<IEnumerable<DeliveryUpdate>> GetUpdatesForPackageAsync(Guid packageId)
        => await _context.DeliveryUpdates.Where(u => u.PackageId == packageId).OrderBy(u => u.Timestamp).ToListAsync();

    public async Task AddAsync(Package package)
    {
        await _context.Packages.AddAsync(package);
        await _context.SaveChangesAsync();
    }

    public async Task AddUpdateAsync(DeliveryUpdate update)
    {
        await _context.DeliveryUpdates.AddAsync(update);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePackageStatusAsync(Package package, DeliveryUpdate update)
    {
        _context.Packages.Update(package);
        await _context.DeliveryUpdates.AddAsync(update);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByTrackingCodeAsync(string trackingCode)
        => await _context.Packages.AnyAsync(p => p.TrackingCode == trackingCode);
}