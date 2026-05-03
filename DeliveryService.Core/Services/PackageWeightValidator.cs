using DeliveryService.Core.Interfaces;

namespace DeliveryService.Core.Services;

public class PackageWeightValidator : IPackageWeightValidator
{
    public bool IsValid(decimal weight) => weight > 0 && weight <= 50;
}
