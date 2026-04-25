using System.Security.Cryptography;
using DeliveryService.Core.Interfaces;

namespace DeliveryService.Infrastructure.Services;

public class TrackingCodeGenerator : ITrackingCodeGenerator
{
    private const int Length = 12;
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Generate()
    {
        return RandomNumberGenerator.GetString(Chars, Length);
    }
}