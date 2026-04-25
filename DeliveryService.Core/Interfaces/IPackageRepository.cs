using DeliveryService.Core.Entities;
using DeliveryService.Core.Enums;

namespace DeliveryService.Core.Interfaces;

public interface IPackageRepository
{
    Task<Package?> GetByIdAsync(Guid id);
    Task<Package?> GetByTrackingCodeAsync(string trackingCode);
    Task<IEnumerable<Package>> GetAllAsync();
    Task<IEnumerable<Package>> GetByStatusAsync(PackageStatus status);
    Task<IEnumerable<DeliveryUpdate>> GetUpdatesForPackageAsync(Guid packageId);
    Task AddAsync(Package package);
    Task AddUpdateAsync(DeliveryUpdate update);
    Task UpdatePackageStatusAsync(Package package, DeliveryUpdate update);
    Task<bool> ExistsByTrackingCodeAsync(string trackingCode);
}