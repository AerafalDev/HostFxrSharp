using HostFxrSharp.Exceptions;
using Shouldly;
using Xunit;

namespace HostFxrSharp.Tests;

/// <summary>
/// Integration tests for the process-wide loading of the native <c>hostfxr</c> library through <see cref="HostFxr"/>.
/// </summary>
/// <remarks>
/// hostfxr load state is process-wide and cannot be reset, so these tests deliberately assume no isolation:
/// they assert only invariants that hold regardless of the order in which tests run and regardless of whether a
/// prior test already loaded the library. Every call into the real host is guarded so that a machine lacking a
/// usable .NET host produces a skipped test rather than a failed one.
/// </remarks>
public sealed class HostFxrLoadingTests
{
    [SkippableFact]
    public void EnsureLoadedMakesLoadedHostFxrPathAnExistingFile()
    {
        var path = EnsureHostFxrLoadedOrSkip();

        path.ShouldNotBeNullOrWhiteSpace();
        File.Exists(path).ShouldBeTrue();
        Path.IsPathRooted(path).ShouldBeTrue();
    }

    [SkippableFact]
    public void EnsureLoadedIsIdempotentAndKeepsSamePath()
    {
        var first = EnsureHostFxrLoadedOrSkip();

        // A second load must be a no-op: it must not throw and must keep the already-resolved path.
        Should.NotThrow(static () => HostFxr.EnsureLoaded());

        HostFxr.LoadedHostFxrPath.ShouldBe(first);
    }

    [SkippableFact]
    public void LoadFromWithAlreadyLoadedPathIsHarmlessNoOp()
    {
        var path = EnsureHostFxrLoadedOrSkip();

        // Once a hostfxr is loaded, LoadFrom short-circuits regardless of the argument, so passing the
        // already-loaded path must be a harmless no-op that leaves the loaded path unchanged.
        Should.NotThrow(() => HostFxr.LoadFrom(path));

        HostFxr.LoadedHostFxrPath.ShouldBe(path);
    }

    private static string EnsureHostFxrLoadedOrSkip()
    {
        try
        {
            HostFxr.EnsureLoaded();
        }
        catch (HostingException ex)
        {
            throw new SkipException($"A usable hostfxr could not be located or loaded in this environment: {ex.Message}");
        }

        var path = HostFxr.LoadedHostFxrPath;
        path.ShouldNotBeNull();
        return path;
    }
}
