using HostFxrSharp.Resolution;

namespace HostFxrSharp.Contexts;

/// <summary>Options common to all host-context initialization calls.</summary>
public sealed class HostContextOptions
{
    /// <summary>
    /// Optional path to the native host (typically the <c>.exe</c>). Passed to CoreCLR as the executable path; it
    /// need not be an executable file (for example a <c>comhost.dll</c> path in COM activation scenarios).
    /// </summary>
    public string? HostPath { get; init; }

    /// <summary>
    /// Optional path to the root of the .NET installation in use. If unset, <c>hostfxr</c> auto-detects it from its
    /// own location (self-contained when <c>coreclr</c> sits next to it; otherwise a relative install layout).
    /// </summary>
    public string? DotNetRoot { get; init; }

    /// <summary>Optional resolution options used when this library needs to locate/load <c>hostfxr</c> first.</summary>
    public HostFxrResolutionOptions? Resolution { get; init; }
}
