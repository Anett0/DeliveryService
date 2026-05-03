using DeliveryService.Core.Enums;
using DeliveryService.Core.Interfaces;

namespace DeliveryService.Core.Services;

public class PackageStatusTransitionValidator : IPackageStatusTransitionValidator
{
    public bool CanTransition(PackageStatus current, PackageStatus next)
    {
        if (current is PackageStatus.Delivered or PackageStatus.Returned)
            return false;

        return next switch
        {
            PackageStatus.PickedUp => current == PackageStatus.Created,
            PackageStatus.InTransit => current == PackageStatus.PickedUp,
            PackageStatus.OutForDelivery => current == PackageStatus.InTransit,
            PackageStatus.Delivered => current == PackageStatus.OutForDelivery,
            PackageStatus.Returned => true,
            _ => false
        };
    }
}
