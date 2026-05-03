using System.ComponentModel.DataAnnotations;

namespace DeliveryService.API.DTOs;

public class CreatePackageDto
{
    public Guid SenderId { get; set; }

    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "Recipient name is required.")]
    public string RecipientName { get; set; } = null!;

    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "Recipient address is required.")]
    public string RecipientAddress { get; set; } = null!;

    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "Recipient phone is required.")]
    public string RecipientPhone { get; set; } = null!;

    public decimal Weight { get; set; }

    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "Dimensions are required.")]
    public string Dimensions { get; set; } = null!;
}
