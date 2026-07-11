using System.Runtime.InteropServices;
using HostFxrSharp.Exceptions;
using HostFxrSharp.Resolution;
using Shouldly;
using Xunit;

namespace HostFxrSharp.Tests;

/// <summary>
/// Integration tests that exercise <see cref="NetHost"/> against the real native <c>nethost</c>
/// component installed on the current machine.
/// </summary>
/// <remarks>
/// Every host call is guarded: when the environment lacks the required hosting bits a
/// <see cref="HostingException"/> is raised (or <see cref="NetHost.TryGetHostFxrPath"/> reports
/// failure), and the test is dynamically skipped via <see cref="SkipException"/> rather than
/// being reported as a failure. This keeps the suite green on machines without a usable .NET
/// installation while still asserting real behaviour where one is present.
/// </remarks>
public sealed class NetHostIntegrationTests
{
    [SkippableFact]
    public void GetHostFxrPathReturnsNonEmptyExistingPath()
    {
        var path = ResolveHostFxrPathOrSkip();

        path.ShouldNotBeNullOrWhiteSpace();
        File.Exists(path).ShouldBeTrue($"expected hostfxr to exist at the resolved path '{path}'.");
    }

    [SkippableFact]
    public void GetHostFxrPathReturnsAbsolutePath()
    {
        var path = ResolveHostFxrPathOrSkip();

        Path.IsPathRooted(path).ShouldBeTrue($"expected an absolute path but got '{path}'.");
    }

    [SkippableFact]
    public void GetHostFxrPathFileNameMatchesCurrentPlatform()
    {
        var path = ResolveHostFxrPathOrSkip();

        Path.GetFileName(path).ShouldBe(ExpectedHostFxrFileName(), StringCompareShould.IgnoreCase);
    }

    [SkippableFact]
    public void GetHostFxrPathWithEmptyOptionsMatchesDefault()
    {
        var withDefault = ResolveHostFxrPathOrSkip();

        string withEmptyOptions;

        try
        {
            withEmptyOptions = NetHost.GetHostFxrPath(new HostFxrResolutionOptions());
        }
        catch (HostingException ex)
        {
            throw new SkipException($"nethost is unavailable in this environment: {ex.Message}");
        }

        // An options object whose members are all null must resolve identically to passing no options.
        withEmptyOptions.ShouldBe(withDefault);
    }

    [SkippableFact]
    public void TryGetHostFxrPathReturnsTrueWithExistingPath()
    {
        var located = NetHost.TryGetHostFxrPath(out var path);

        if (!located)
            throw new SkipException("nethost is unavailable in this environment; TryGetHostFxrPath returned false.");

        // [NotNullWhen(true)] guarantees a non-null path once located is true.
        path.ShouldNotBeNullOrWhiteSpace();
        File.Exists(path).ShouldBeTrue($"expected hostfxr to exist at the resolved path '{path}'.");
        Path.GetFileName(path).ShouldBe(ExpectedHostFxrFileName(), StringCompareShould.IgnoreCase);
    }

    [SkippableFact]
    public void TryGetHostFxrPathAgreesWithGetHostFxrPath()
    {
        var expected = ResolveHostFxrPathOrSkip();

        var located = NetHost.TryGetHostFxrPath(out var path);
        located.ShouldBeTrue("GetHostFxrPath succeeded, so the Try variant must also succeed.");
        path.ShouldBe(expected);
    }

    /// <summary>
    /// Resolves the <c>hostfxr</c> path, translating any <see cref="HostingException"/> (for example
    /// <see cref="HostFxrNotFoundException"/> on a machine without a usable .NET installation) into a
    /// dynamic skip. Because <see cref="SkipException"/> never returns, callers can treat the
    /// return value as a valid, resolved path.
    /// </summary>
    private static string ResolveHostFxrPathOrSkip()
    {
        try
        {
            return NetHost.GetHostFxrPath();
        }
        catch (HostingException ex)
        {
            throw new SkipException($"nethost is unavailable in this environment: {ex.Message}");
        }
    }

    /// <summary>Returns the expected <c>hostfxr</c> library file name for the current platform.</summary>
    private static string ExpectedHostFxrFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "hostfxr.dll";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "libhostfxr.dylib";

        return "libhostfxr.so";
    }
}
