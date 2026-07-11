namespace HostFxrSharp.Exceptions;

/// <summary>Thrown when a requested runtime property does not exist.</summary>
public sealed class RuntimePropertyNotFoundException : HostingApiException
{
    /// <summary>Gets the name of the property that was not found, when known.</summary>
    public string? PropertyName { get; init; }

    /// <summary>Initializes a new instance of the <see cref="RuntimePropertyNotFoundException"/> class.</summary>
    public RuntimePropertyNotFoundException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RuntimePropertyNotFoundException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public RuntimePropertyNotFoundException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RuntimePropertyNotFoundException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RuntimePropertyNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>Creates an exception for a specific missing property name.</summary>
    /// <param name="propertyName">The name of the missing runtime property.</param>
    /// <returns>A configured <see cref="RuntimePropertyNotFoundException"/>.</returns>
    public static RuntimePropertyNotFoundException ForProperty(string propertyName)
    {
        return new RuntimePropertyNotFoundException($"The runtime property '{propertyName}' was not found.") { PropertyName = propertyName };
    }
}
