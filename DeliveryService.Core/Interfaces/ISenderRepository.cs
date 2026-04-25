using DeliveryService.Core.Entities;

namespace DeliveryService.Core.Interfaces;

public interface ISenderRepository
{
    Task<Sender?> GetByIdAsync(Guid id);
    Task<IEnumerable<Package>> GetPackagesBySenderIdAsync(Guid senderId);
}