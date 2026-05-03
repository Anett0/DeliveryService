using DeliveryService.Core.Enums;
using DeliveryService.Core.Services;
using Xunit;

namespace DeliveryService.Tests.Unit;

public class PackageStatusValidatorTests
{
    private readonly PackageStatusTransitionValidator _validator = new();

    [Theory]
    [InlineData(PackageStatus.Created, PackageStatus.PickedUp, true)]
    [InlineData(PackageStatus.Created, PackageStatus.InTransit, false)]
    [InlineData(PackageStatus.PickedUp, PackageStatus.InTransit, true)]
    [InlineData(PackageStatus.InTransit, PackageStatus.OutForDelivery, true)]
    [InlineData(PackageStatus.OutForDelivery, PackageStatus.Delivered, true)]
    [InlineData(PackageStatus.Created, PackageStatus.Returned, true)]
    [InlineData(PackageStatus.PickedUp, PackageStatus.Returned, true)]
    [InlineData(PackageStatus.InTransit, PackageStatus.Returned, true)]
    [InlineData(PackageStatus.OutForDelivery, PackageStatus.Returned, true)]
    [InlineData(PackageStatus.Delivered, PackageStatus.Returned, false)]
    [InlineData(PackageStatus.Returned, PackageStatus.PickedUp, false)]
    [InlineData(PackageStatus.Returned, PackageStatus.Returned, false)]
    [InlineData(PackageStatus.Delivered, PackageStatus.Created, false)]
    public void Transition_ShouldBeValid(PackageStatus current, PackageStatus next, bool expected)
    {
        Assert.Equal(expected, _validator.CanTransition(current, next));
    }
}
