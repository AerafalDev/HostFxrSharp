using System.Runtime.InteropServices;
using HostFxrSharp.Interop;
using Shouldly;
using Xunit;

namespace HostFxrSharp.Tests;

public sealed class NativeLibraryResolverTests
{
    [Theory]
    [InlineData("nethost")]
    [InlineData("hostfxr")]
    [InlineData("foo")]
    public void GetNativeLibraryFileNameProducesPlatformCorrectName(string baseName)
    {
        var expected = ExpectedNativeLibraryFileName(baseName);

        NativeLibraryResolver.GetNativeLibraryFileName(baseName).ShouldBe(expected);
    }

    [Theory]
    [InlineData("nethost")]
    [InlineData("hostfxr")]
    [InlineData("foo")]
    public void GetNativeLibraryFileNameContainsBaseName(string baseName)
    {
        var fileName = NativeLibraryResolver.GetNativeLibraryFileName(baseName);

        fileName.ShouldNotBeNullOrEmpty();
        fileName.ShouldContain(baseName);
    }

    [Theory]
    [InlineData("nethost")]
    [InlineData("hostfxr")]
    public void GetNativeLibraryFileNameUsesPlatformPrefixAndExtension(string baseName)
    {
        var fileName = NativeLibraryResolver.GetNativeLibraryFileName(baseName);

        if (OperatingSystem.IsWindows())
        {
            fileName.ShouldBe($"{baseName}.dll");
            fileName.ShouldStartWith(baseName);
            fileName.ShouldEndWith(".dll");
        }
        else if (OperatingSystem.IsMacOS())
        {
            fileName.ShouldBe($"lib{baseName}.dylib");
            fileName.ShouldStartWith("lib");
            fileName.ShouldEndWith(".dylib");
        }
        else
        {
            fileName.ShouldBe($"lib{baseName}.so");
            fileName.ShouldStartWith("lib");
            fileName.ShouldEndWith(".so");
        }
    }

    [Fact]
    public void GetNativeLibraryFileNameIsDeterministicAcrossCalls()
    {
        var first = NativeLibraryResolver.GetNativeLibraryFileName("nethost");

        for (var i = 0; i < 5; i++)
        {
            NativeLibraryResolver.GetNativeLibraryFileName("nethost").ShouldBe(first);
        }
    }

    [Fact]
    public void GetRidReturnsOsDashArch()
    {
        var rid = NativeLibraryResolver.GetRid();

        rid.ShouldNotBeNullOrEmpty();

        var parts = rid.Split('-');
        parts.Length.ShouldBe(2);
        parts[0].ShouldNotBeNullOrEmpty();
        parts[1].ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void GetRidOsComponentMatchesCurrentOperatingSystem()
    {
        var os = NativeLibraryResolver.GetRid().Split('-')[0];

        os.ShouldBe(ExpectedOs());
        os.ShouldBeOneOf("win", "linux", "osx");
    }

    [Fact]
    public void GetRidArchComponentIsKnownArchitecture()
    {
        var arch = NativeLibraryResolver.GetRid().Split('-')[1];

        arch.ShouldBeOneOf("x64", "x86", "arm64", "arm");
    }

    [Fact]
    public void GetRidArchComponentMatchesProcessArchitecture()
    {
        NativeLibraryResolver.GetRid().Split('-')[1].ShouldBe(ExpectedArch());
    }

    [Fact]
    public void GetRidMatchesCurrentOsAndArchitecture()
    {
        NativeLibraryResolver.GetRid().ShouldBe($"{ExpectedOs()}-{ExpectedArch()}");
    }

    [Fact]
    public void GetRidDoesNotIncludeMuslModifier()
    {
        NativeLibraryResolver.GetRid().ShouldNotContain("musl");
    }

    [Fact]
    public void GetRidIsDeterministicAcrossCalls()
    {
        var first = NativeLibraryResolver.GetRid();

        for (var i = 0; i < 5; i++)
        {
            NativeLibraryResolver.GetRid().ShouldBe(first);
        }
    }

    private static string ExpectedNativeLibraryFileName(string baseName)
    {
        if (OperatingSystem.IsWindows())
            return $"{baseName}.dll";

        if (OperatingSystem.IsMacOS())
            return $"lib{baseName}.dylib";

        return $"lib{baseName}.so";
    }

    private static string ExpectedOs()
    {
        if (OperatingSystem.IsWindows())
            return "win";

        if (OperatingSystem.IsMacOS())
            return "osx";

        return "linux";
    }

    private static string ExpectedArch()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "x64"
        };
    }
}
