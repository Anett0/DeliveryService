namespace DeliveryService.Core.Interfaces;

public interface IPackageWeightValidator
{
    bool IsValid(decimal weight);
}
