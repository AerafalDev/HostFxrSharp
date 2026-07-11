namespace HostFxrSharp.Exceptions;

/// <summary>
/// Thrown when a new <c>hostfxr</c> API is invoked against an older <c>hostpolicy</c> that does not
/// implement it (native status <see cref="HostStatusCode.HostApiUnsupportedVersion"/>).
/// </summary>
public sealed class IncompatibleHostPolicyException : HostingApiException
{
    /// <summary>Initializes a new instance of the <see cref="IncompatibleHostPolicyException"/> class.</summary>
    public IncompatibleHostPolicyException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="IncompatibleHostPolicyException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public IncompatibleHostPolicyException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="IncompatibleHostPolicyException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public IncompatibleHostPolicyException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="IncompatibleHostPolicyException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="statusCode">The native status code that caused the error.</param>
    public IncompatibleHostPolicyException(string message, HostStatusCode statusCode) : base(message, statusCode)
    {
    }
}
