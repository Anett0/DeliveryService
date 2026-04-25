using DeliveryService.Core.Enums;

namespace DeliveryService.Core.Entities;

public class DeliveryUpdate
{
    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public PackageStatus Status { get; set; }
    public string Location { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime Timestamp { get; set; }
    public string UpdatedBy { get; set; } = null!;

    public Package Package { get; set; } = null!;
}