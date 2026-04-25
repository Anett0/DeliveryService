namespace DeliveryService.Core.Entities;

public class Sender
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Address { get; set; } = null!;

    public ICollection<Package> Packages { get; set; } = new List<Package>();
}