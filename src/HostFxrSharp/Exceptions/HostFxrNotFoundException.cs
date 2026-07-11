namespace HostFxrSharp.Exceptions;

/// <summary>Thrown when the <c>hostfxr</c> library cannot be located by <c>nethost</c>.</summary>
public sealed class HostFxrNotFoundException : HostingException
{
    /// <summary>Initializes a new instance of the <see cref="HostFxrNotFoundException"/> class.</summary>
    public HostFxrNotFoundException() : base("The hostfxr library could not be located. Ensure a compatible .NET installation is present, or that nethost.dll ships with the application.")
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostFxrNotFoundException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public HostFxrNotFoundException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="HostFxrNotFoundException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public HostFxrNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
