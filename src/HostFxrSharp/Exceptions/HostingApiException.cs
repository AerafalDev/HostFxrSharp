namespace HostFxrSharp.Exceptions;

/// <summary>A general failure returned by a native hosting call, carrying its <see cref="HostStatusCode"/>.</summary>
public class HostingApiException : HostingException
{
    /// <summary>Initializes a new instance of the <see cref="HostingApiException"/> class.</summary>
    public HostingApiException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostingApiException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public HostingApiException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostingApiException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public HostingApiException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostingApiException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="statusCode">The native status code that caused the error.</param>
    public HostingApiException(string message, HostStatusCode statusCode) : base(message, statusCode)
    {
    }
}
