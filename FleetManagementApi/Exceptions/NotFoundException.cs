namespace FleetManagementApi.Exceptions;

/// <summary>
/// Thrown when a requested resource (e.g. vehicle, driver) does not exist.
/// Handled by ApiExceptionHandler → 404 Not Found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception inner) : base(message, inner) { }
}
