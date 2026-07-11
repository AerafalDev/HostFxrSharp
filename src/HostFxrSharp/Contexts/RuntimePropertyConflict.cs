namespace HostFxrSharp.Contexts;

/// <summary>Describes a runtime property whose requested value differs from the value already set on the running runtime.</summary>
/// <param name="Name">The runtime property name.</param>
/// <param name="RequestedValue">The value requested by the secondary context's runtime configuration.</param>
/// <param name="ActiveValue">The value currently set on the running runtime, or <see langword="null"/> if the property is not set there.</param>
public readonly record struct RuntimePropertyConflict(string Name, string RequestedValue, string? ActiveValue);
