namespace HostFxrSharp.Exceptions;

/// <summary>Thrown when the requested scenario is not supported for a given host context.</summary>
public sealed class UnsupportedHostingScenarioException : HostingApiException
{
    /// <summary>Initializes a new instance of the <see cref="UnsupportedHostingScenarioException"/> class.</summary>
    public UnsupportedHostingScenarioException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="UnsupportedHostingScenarioException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public UnsupportedHostingScenarioException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="UnsupportedHostingScenarioException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public UnsupportedHostingScenarioException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="UnsupportedHostingScenarioException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="statusCode">The native status code that caused the error.</param>
    public UnsupportedHostingScenarioException(string message, HostStatusCode statusCode) : base(message, statusCode)
    {
    }
}
