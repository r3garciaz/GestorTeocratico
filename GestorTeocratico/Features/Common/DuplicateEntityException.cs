namespace GestorTeocratico.Features.Common;

/// <summary>
/// Thrown when an operation would violate a uniqueness rule of the domain
/// (for example, creating a congregation when one already exists, or a
/// department/responsibility/publisher with a duplicate name).
/// </summary>
public sealed class DuplicateEntityException : Exception
{
    public DuplicateEntityException(string message)
        : base(message)
    {
    }

    public DuplicateEntityException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
