namespace HostFxrSharp.Exceptions;

/// <summary>Base type for all exceptions raised by the NativeHosting library.</summary>
/// <remarks>
/// Native status codes are never surfaced as raw integers on the happy path. When an operation
/// fails, a typed <see cref="HostingException"/> subtype is thrown; the originating
/// <see cref="HostStatusCode"/> (when available) is preserved on <see cref="StatusCode"/> for
/// diagnostics.
/// </remarks>
public abstract class HostingException : Exception
{
    /// <summary>Gets the originating native status code, when the failure came from a native call.</summary>
    public HostStatusCode? StatusCode { get; }

    /// <summary>Initializes a new instance of the <see cref="HostingException"/> class.</summary>
    protected HostingException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostingException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    protected HostingException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostingException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    protected HostingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostingException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="statusCode">The native status code that caused the error.</param>
    protected HostingException(string message, HostStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
