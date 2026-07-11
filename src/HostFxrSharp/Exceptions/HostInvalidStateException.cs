namespace HostFxrSharp.Exceptions;

/// <summary>Thrown when a native call reports an invalid host state (<see cref="HostStatusCode.HostInvalidState"/>).</summary>
public sealed class HostInvalidStateException : HostingApiException
{
    /// <summary>Initializes a new instance of the <see cref="HostInvalidStateException"/> class.</summary>
    public HostInvalidStateException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostInvalidStateException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public HostInvalidStateException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostInvalidStateException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public HostInvalidStateException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostInvalidStateException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="statusCode">The native status code that caused the error.</param>
    public HostInvalidStateException(string message, HostStatusCode statusCode) : base(message, statusCode)
    {
    }
}
