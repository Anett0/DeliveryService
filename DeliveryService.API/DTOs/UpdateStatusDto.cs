using System.ComponentModel.DataAnnotations;

namespace DeliveryService.API.DTOs;

public class UpdateStatusDto
{
    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "Status is required.")]
    public string Status { get; set; } = null!;

    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "Location is required.")]
    public string Location { get; set; } = null!;

    public string? Notes { get; set; }

    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "UpdatedBy is required.")]
    public string UpdatedBy { get; set; } = null!;
}
