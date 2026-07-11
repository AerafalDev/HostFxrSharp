namespace HostFxrSharp.Exceptions;

/// <summary>Thrown when <c>nethost</c> fails to resolve <c>hostfxr</c> for a non "not-found" reason.</summary>
public sealed class HostFxrResolutionException : HostingException
{
    /// <summary>Initializes a new instance of the <see cref="HostFxrResolutionException"/> class.</summary>
    public HostFxrResolutionException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostFxrResolutionException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public HostFxrResolutionException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostFxrResolutionException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public HostFxrResolutionException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostFxrResolutionException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="statusCode">The native status code that caused the error.</param>
    public HostFxrResolutionException(string message, HostStatusCode statusCode) : base(message, statusCode)
    {
    }
}
