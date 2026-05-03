using DeliveryService.Infrastructure.Services;
using Xunit;

namespace DeliveryService.Tests.Unit;

public class TrackingCodeGeneratorTests
{
    private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private readonly TrackingCodeGenerator _generator = new();

    [Fact]
    public void Generate_ShouldReturnTwelveCharacters()
    {
        var trackingCode = _generator.Generate();

        Assert.Equal(12, trackingCode.Length);
    }

    [Fact]
    public void Generate_ShouldUseOnlyUppercaseLettersAndDigits()
    {
        var trackingCode = _generator.Generate();

        Assert.All(trackingCode, character => Assert.Contains(character, AllowedCharacters));
    }

    [Fact]
    public void Generate_ShouldReturnDifferentValuesAcrossMultipleCalls()
    {
        var trackingCodes = Enumerable.Range(0, 100)
            .Select(_ => _generator.Generate())
            .ToHashSet();

        Assert.True(trackingCodes.Count > 1);
    }
}
