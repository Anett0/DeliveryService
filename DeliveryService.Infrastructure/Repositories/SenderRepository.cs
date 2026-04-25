using DeliveryService.Core.Entities;
using DeliveryService.Core.Interfaces;
using DeliveryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Infrastructure.Repositories;

public class SenderRepository : ISenderRepository
{
    private readonly AppDbContext _context;

    public SenderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Sender?> GetByIdAsync(Guid id)
        => await _context.Senders.Include(s => s.Packages).FirstOrDefaultAsync(s => s.Id == id);

    public async Task<IEnumerable<Package>> GetPackagesBySenderIdAsync(Guid senderId)
        => await _context.Packages.Where(p => p.SenderId == senderId).Include(p => p.Updates).ToListAsync();
}