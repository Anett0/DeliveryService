namespace DeliveryService.API.DTOs;

public class DeliveryUpdateDto
{
    public string Status { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime Timestamp { get; set; }
    public string UpdatedBy { get; set; } = null!;
}