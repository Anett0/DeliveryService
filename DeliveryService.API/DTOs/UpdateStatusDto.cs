namespace DeliveryService.API.DTOs;

public class UpdateStatusDto
{
    public string Status { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string? Notes { get; set; }
    public string UpdatedBy { get; set; } = null!;
}