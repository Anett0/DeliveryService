using DeliveryService.Core.Services;
using Xunit;

namespace DeliveryService.Tests.Unit;

public class WeightValidatorTests
{
    private readonly PackageWeightValidator _validator = new();

    [Theory]
    [InlineData(0.1, true)]
    [InlineData(50, true)]
    [InlineData(25.5, true)]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    [InlineData(50.1, false)]
    public void Weight_ShouldBeValid(decimal weight, bool expected)
    {
        Assert.Equal(expected, _validator.IsValid(weight));
    }
}
