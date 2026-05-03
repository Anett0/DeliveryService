using DeliveryService.Core.Enums;

namespace DeliveryService.Core.Interfaces;

public interface IPackageStatusTransitionValidator
{
    bool CanTransition(PackageStatus current, PackageStatus next);
}
