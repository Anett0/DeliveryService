using DeliveryService.Core.Enums;
using Xunit;

namespace DeliveryService.Tests.Unit;

public class PackageStatusValidatorTests
{
    private bool IsValidTransition(PackageStatus current, PackageStatus next)
    {
        return next switch
        {
            PackageStatus.PickedUp => current == PackageStatus.Created,
            PackageStatus.InTransit => current == PackageStatus.PickedUp,
            PackageStatus.OutForDelivery => current == PackageStatus.InTransit,
            PackageStatus.Delivered => current == PackageStatus.OutForDelivery,
            PackageStatus.Returned => current != PackageStatus.Delivered,
            _ => false
        };
    }

    [Theory]
    [InlineData(PackageStatus.Created, PackageStatus.PickedUp, true)]
    [InlineData(PackageStatus.Created, PackageStatus.InTransit, false)]
    [InlineData(PackageStatus.PickedUp, PackageStatus.InTransit, true)]
    [InlineData(PackageStatus.InTransit, PackageStatus.OutForDelivery, true)]
    [InlineData(PackageStatus.OutForDelivery, PackageStatus.Delivered, true)]
    [InlineData(PackageStatus.Delivered, PackageStatus.Returned, false)]
    [InlineData(PackageStatus.Delivered, PackageStatus.Created, false)]
    public void Transition_ShouldBeValid(PackageStatus current, PackageStatus next, bool expected)
    {
        Assert.Equal(expected, IsValidTransition(current, next));
    }
}