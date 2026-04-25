namespace DeliveryService.API.DTOs;

public class CreatePackageDto
{
    public Guid SenderId { get; set; }
    public string RecipientName { get; set; } = null!;
    public string RecipientAddress { get; set; } = null!;
    public string RecipientPhone { get; set; } = null!;
    public decimal Weight { get; set; }
    public string Dimensions { get; set; } = null!;
}