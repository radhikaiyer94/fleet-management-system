namespace FleetManagementApi.Exceptions;

/// <summary>
/// Thrown when the request is invalid (e.g. business rule violation, invalid operation).
/// Handled by ApiExceptionHandler → 400 Bad Request.
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }

    public BadRequestException(string message, Exception inner) : base(message, inner) { }
}
