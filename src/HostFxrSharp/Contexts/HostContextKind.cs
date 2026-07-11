namespace HostFxrSharp.Contexts;

/// <summary>Identifies whether a host context is the process's first (runtime-owning) context or a secondary one.</summary>
/// <remarks>
/// Only one runtime can be loaded per process. The first initialized host context owns that runtime; any
/// subsequent context <em>attaches</em> to it and is a <see cref="Secondary"/> context. See
/// <c>docs/RESEARCH.md</c> and the design document's "Multiple host contexts interactions" section.
/// </remarks>
public enum HostContextKind
{
    /// <summary>The first host context in the process; it loads and owns the runtime.</summary>
    First,

    /// <summary>A secondary host context that attached to the already-loaded runtime.</summary>
    Secondary
}
