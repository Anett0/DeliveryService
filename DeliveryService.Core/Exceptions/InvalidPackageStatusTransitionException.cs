namespace DeliveryService.Core.Exceptions;

public class InvalidPackageStatusTransitionException : Exception
{
    public InvalidPackageStatusTransitionException(string message) : base(message) { }
}