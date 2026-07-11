using HostFxrSharp.Contexts;
using HostFxrSharp.Resolution;
using Shouldly;
using Xunit;

namespace HostFxrSharp.Tests;

/// <summary>
/// Deterministic unit tests for the option record types <see cref="HostContextOptions"/> and
/// <see cref="HostFxrResolutionOptions"/>: default nullability and init-only round-trips.
/// </summary>
public sealed class OptionsTests
{
    // ---------------------------------------------------------------------
    // HostContextOptions
    // ---------------------------------------------------------------------

    [Fact]
    public void HostContextOptionsDefaultInstanceHasAllPropertiesNull()
    {
        var options = new HostContextOptions();

        options.HostPath.ShouldBeNull();
        options.DotNetRoot.ShouldBeNull();
        options.Resolution.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("apphost")]
    [InlineData("/usr/lib/app/comhost.dll")]
    [InlineData(@"C:\Program Files\App\app.exe")]
    public void HostContextOptionsHostPathRoundTripsViaObjectInitializer(string hostPath)
    {
        var options = new HostContextOptions { HostPath = hostPath };

        options.HostPath.ShouldBe(hostPath);
        options.DotNetRoot.ShouldBeNull();
        options.Resolution.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("dotnet-root")]
    [InlineData("/usr/share/dotnet")]
    [InlineData(@"C:\Program Files\dotnet")]
    public void HostContextOptionsDotNetRootRoundTripsViaObjectInitializer(string dotNetRoot)
    {
        var options = new HostContextOptions { DotNetRoot = dotNetRoot };

        options.DotNetRoot.ShouldBe(dotNetRoot);
        options.HostPath.ShouldBeNull();
        options.Resolution.ShouldBeNull();
    }

    [Fact]
    public void HostContextOptionsResolutionRoundTripsViaObjectInitializer()
    {
        var resolution = new HostFxrResolutionOptions();

        var options = new HostContextOptions { Resolution = resolution };

        options.Resolution.ShouldBeSameAs(resolution);
        options.HostPath.ShouldBeNull();
        options.DotNetRoot.ShouldBeNull();
    }

    [Fact]
    public void HostContextOptionsAllPropertiesRoundTripTogether()
    {
        var resolution = new HostFxrResolutionOptions
        {
            AssemblyPath = "component.dll",
            DotNetRoot = "/opt/dotnet",
        };

        var options = new HostContextOptions
        {
            HostPath = "host.exe",
            DotNetRoot = "/opt/dotnet-root",
            Resolution = resolution,
        };

        options.HostPath.ShouldBe("host.exe");
        options.DotNetRoot.ShouldBe("/opt/dotnet-root");
        options.Resolution.ShouldBeSameAs(resolution);
        options.Resolution!.AssemblyPath.ShouldBe("component.dll");
        options.Resolution.DotNetRoot.ShouldBe("/opt/dotnet");
    }

    // ---------------------------------------------------------------------
    // HostFxrResolutionOptions
    // ---------------------------------------------------------------------

    [Fact]
    public void HostFxrResolutionOptionsDefaultInstanceHasAllPropertiesNull()
    {
        var options = new HostFxrResolutionOptions();

        options.AssemblyPath.ShouldBeNull();
        options.DotNetRoot.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("component.dll")]
    [InlineData("/usr/lib/app/component.dll")]
    [InlineData(@"C:\App\component.dll")]
    public void HostFxrResolutionOptionsAssemblyPathRoundTripsViaObjectInitializer(string assemblyPath)
    {
        var options = new HostFxrResolutionOptions { AssemblyPath = assemblyPath };

        options.AssemblyPath.ShouldBe(assemblyPath);
        options.DotNetRoot.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("dotnet-root")]
    [InlineData("/usr/share/dotnet")]
    [InlineData(@"C:\Program Files\dotnet")]
    public void HostFxrResolutionOptionsDotNetRootRoundTripsViaObjectInitializer(string dotNetRoot)
    {
        var options = new HostFxrResolutionOptions { DotNetRoot = dotNetRoot };

        options.DotNetRoot.ShouldBe(dotNetRoot);
        options.AssemblyPath.ShouldBeNull();
    }

    [Fact]
    public void HostFxrResolutionOptionsBothPropertiesRoundTripTogether()
    {
        var options = new HostFxrResolutionOptions
        {
            AssemblyPath = "component.dll",
            DotNetRoot = "/opt/dotnet",
        };

        options.AssemblyPath.ShouldBe("component.dll");
        options.DotNetRoot.ShouldBe("/opt/dotnet");
    }
}
