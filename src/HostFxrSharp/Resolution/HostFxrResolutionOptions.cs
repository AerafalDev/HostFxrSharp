namespace HostFxrSharp.Resolution;

/// <summary>Options controlling how <c>nethost</c> locates the <c>hostfxr</c> library.</summary>
/// <remarks>
/// If both properties are <see langword="null"/>, <c>hostfxr</c> is located via the environment variable or
/// global registration. If <see cref="DotNetRoot"/> is set it takes precedence and
/// <see cref="AssemblyPath"/> is ignored (matching native semantics — see <c>docs/RESEARCH.md §2</c>).
/// </remarks>
public sealed class HostFxrResolutionOptions
{
    /// <summary>
    /// Optional path to the component's assembly. When set (and <see cref="DotNetRoot"/> is not), <c>hostfxr</c>
    /// is located as if <see cref="AssemblyPath"/> were an <c>apphost</c>.
    /// </summary>
    public string? AssemblyPath { get; init; }

    /// <summary>
    /// Optional path to the root of a .NET installation (the folder containing <c>dotnet</c>). When set, <c>hostfxr</c>
    /// is located as if starting <c>dotnet app.dll</c>, and <see cref="AssemblyPath"/> is ignored.
    /// </summary>
    public string? DotNetRoot { get; init; }
}
