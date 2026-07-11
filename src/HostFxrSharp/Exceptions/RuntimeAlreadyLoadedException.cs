namespace HostFxrSharp.Exceptions;

/// <summary>Thrown when an operation requires that no runtime be loaded, but one already is.</summary>
public sealed class RuntimeAlreadyLoadedException : HostingApiException
{
    /// <summary>Initializes a new instance of the <see cref="RuntimeAlreadyLoadedException"/> class.</summary>
    public RuntimeAlreadyLoadedException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RuntimeAlreadyLoadedException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public RuntimeAlreadyLoadedException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RuntimeAlreadyLoadedException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RuntimeAlreadyLoadedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RuntimeAlreadyLoadedException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="statusCode">The native status code that caused the error.</param>
    public RuntimeAlreadyLoadedException(string message, HostStatusCode statusCode) : base(message, statusCode)
    {
    }
}
