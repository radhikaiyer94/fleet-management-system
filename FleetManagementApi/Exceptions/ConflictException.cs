namespace FleetManagementApi.Exceptions;

/// <summary>
/// Thrown when a resource conflict occurs (e.g. duplicate resource).
/// Handled by ApiExceptionHandler → 409 Conflict.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }

    public ConflictException(string message, Exception inner) : base(message, inner) { }
}