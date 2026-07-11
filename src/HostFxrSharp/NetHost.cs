using System.Diagnostics.CodeAnalysis;
using HostFxrSharp.Exceptions;
using HostFxrSharp.Resolution;

namespace HostFxrSharp;

/// <summary>Locates the <c>hostfxr</c> library via the native <c>nethost</c> component.</summary>
/// <remarks>
/// This is the entry point for the "locate hosting components" step. The size→fill buffer protocol of
/// <c>get_hostfxr_path</c> is handled internally (stack buffer first, then pooled growth on
/// <see cref="HostStatusCode.HostApiBufferTooSmall"/>); callers just receive a <see cref="string"/>.
/// </remarks>
public static class NetHost
{
    /// <summary>Locates the <c>hostfxr</c> library and returns its full path.</summary>
    /// <param name="options">Optional resolution options.</param>
    /// <returns>The absolute path to <c>hostfxr</c>.</returns>
    /// <exception cref="HostFxrNotFoundException"><c>hostfxr</c> could not be located.</exception>
    /// <exception cref="HostFxrResolutionException">Resolution failed for another reason.</exception>
    public static string GetHostFxrPath(HostFxrResolutionOptions? options = null)
    {
        return NetHostOperations.GetHostFxrPath(options?.AssemblyPath, options?.DotNetRoot);
    }

    /// <summary>Attempts to locate the <c>hostfxr</c> library.</summary>
    /// <param name="path">The resolved path on success; otherwise <see langword="null"/>.</param>
    /// <param name="options">Optional resolution options.</param>
    /// <returns><see langword="true"/> if <c>hostfxr</c> was located; otherwise <see langword="false"/>.</returns>
    public static bool TryGetHostFxrPath([NotNullWhen(true)] out string? path, HostFxrResolutionOptions? options = null)
    {
        try
        {
            path = GetHostFxrPath(options);
            return true;
        }
        catch (HostingException)
        {
            path = null;
            return false;
        }
    }
}
