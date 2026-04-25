using DeliveryService.Core.Enums;

namespace DeliveryService.Core.Entities;

public class Package
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string RecipientName { get; set; } = null!;
    public string RecipientAddress { get; set; } = null!;
    public string RecipientPhone { get; set; } = null!;
    public decimal Weight { get; set; }
    public string Dimensions { get; set; } = null!;
    public PackageStatus Status { get; set; }
    public string TrackingCode { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public Sender Sender { get; set; } = null!;
    public ICollection<DeliveryUpdate> Updates { get; set; } = new List<DeliveryUpdate>();
}