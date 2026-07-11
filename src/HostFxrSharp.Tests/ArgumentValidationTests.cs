using Shouldly;
using Xunit;

namespace HostFxrSharp.Tests;

/// <summary>
/// Verifies the public argument-validation contracts on <see cref="HostFxr"/>. Every case exercised here
/// throws from the guard clause that runs before any native work (<c>EnsureLoaded</c> / P/Invoke), so the
/// tests are fully deterministic and require no located hostfxr or loaded runtime on any platform.
/// </summary>
public sealed class ArgumentValidationTests
{
    [Fact]
    public void LoadFromWithNullThrowsArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(static () => HostFxr.LoadFrom(null!));

        exception.ParamName.ShouldBe("hostFxrPath");
    }

    [Fact]
    public void LoadFromWithEmptyThrowsArgumentException()
    {
        var exception = Should.Throw<ArgumentException>(static () => HostFxr.LoadFrom(string.Empty));

        exception.ParamName.ShouldBe("hostFxrPath");
    }

    [Fact]
    public void InitializeForCommandLineWithNullArgsThrowsArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(static () => HostFxr.InitializeForCommandLine(null!));

        exception.ParamName.ShouldBe("args");
    }

    [Fact]
    public void InitializeForRuntimeConfigWithNullThrowsArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(static () => HostFxr.InitializeForRuntimeConfig(null!));

        exception.ParamName.ShouldBe("runtimeConfigPath");
    }

    [Fact]
    public void InitializeForRuntimeConfigWithEmptyThrowsArgumentException()
    {
        var exception = Should.Throw<ArgumentException>(static () => HostFxr.InitializeForRuntimeConfig(string.Empty));

        exception.ParamName.ShouldBe("runtimeConfigPath");
    }

    [Fact]
    public void TryGetActiveRuntimePropertyWithNullNameThrowsArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(static () => HostFxr.TryGetActiveRuntimeProperty(null!, out _));

        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void TryGetActiveRuntimePropertyWithEmptyNameThrowsArgumentException()
    {
        var exception = Should.Throw<ArgumentException>(static () => HostFxr.TryGetActiveRuntimeProperty(string.Empty, out _));

        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void SetErrorWriterWithNullThrowsArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(static () => HostFxr.SetErrorWriter(null!));

        exception.ParamName.ShouldBe("writer");
    }
}
