using HostFxrSharp.Loading;
using Shouldly;
using Xunit;

namespace HostFxrSharp.Tests;

/// <summary>
/// Deterministic unit tests for the four loader value types
/// (<see cref="AssemblyLoader"/>, <see cref="AssemblyFunctionPointerLoader"/>,
/// <see cref="FunctionPointerResolver"/> and <see cref="AssemblyBytesLoader"/>)
/// in their default (invalid) state. These require no runtime host, native
/// library, or files and behave identically on Windows, Linux and macOS.
/// </summary>
public sealed class LoaderStructsTests
{
    // -------------------------------------------------------------------------
    // AssemblyLoader
    // -------------------------------------------------------------------------

    [Fact]
    public void AssemblyLoaderDefaultIsNotValid()
    {
        var loader = default(AssemblyLoader);

        loader.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void AssemblyLoaderDefaultUnderlyingPointerIsZero()
    {
        var loader = default(AssemblyLoader);

        loader.UnderlyingPointer.ShouldBe(nint.Zero);
    }

    [Fact]
    public void AssemblyLoaderTwoDefaultsAreEqual()
    {
        var left = default(AssemblyLoader);
        var right = default(AssemblyLoader);

        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
    }

    [Fact]
    public void AssemblyLoaderTwoDefaultsHaveEqualHashCodes()
    {
        var left = default(AssemblyLoader);
        var right = default(AssemblyLoader);

        left.GetHashCode().ShouldBe(right.GetHashCode());
        left.GetHashCode().ShouldBe(nint.Zero.GetHashCode());
    }

    [Fact]
    public void AssemblyLoaderDefaultEqualsBoxedDefault()
    {
        var loader = default(AssemblyLoader);
        object boxed = default(AssemblyLoader);

        loader.Equals(boxed).ShouldBeTrue();
    }

    [Fact]
    public void AssemblyLoaderDefaultDoesNotEqualNullOrOtherType()
    {
        var loader = default(AssemblyLoader);

        loader.Equals(null).ShouldBeFalse();
        loader.Equals("not a loader").ShouldBeFalse();
    }

    [Fact]
    public void AssemblyLoaderLoadOnDefaultThrowsInvalidOperation()
    {
        Should.Throw<InvalidOperationException>(static () => default(AssemblyLoader).Load("path/to/assembly.dll"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AssemblyLoaderLoadWithNullOrEmptyPathThrowsArgument(string? assemblyPath)
    {
        Should.Throw<ArgumentException>(() => default(AssemblyLoader).Load(assemblyPath!));
    }

    // -------------------------------------------------------------------------
    // AssemblyFunctionPointerLoader
    // -------------------------------------------------------------------------

    [Fact]
    public void AssemblyFunctionPointerLoaderDefaultIsNotValid()
    {
        var loader = default(AssemblyFunctionPointerLoader);

        loader.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void AssemblyFunctionPointerLoaderDefaultUnderlyingPointerIsZero()
    {
        var loader = default(AssemblyFunctionPointerLoader);

        loader.UnderlyingPointer.ShouldBe(nint.Zero);
    }

    [Fact]
    public void AssemblyFunctionPointerLoaderTwoDefaultsAreEqual()
    {
        var left = default(AssemblyFunctionPointerLoader);
        var right = default(AssemblyFunctionPointerLoader);

        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
    }

    [Fact]
    public void AssemblyFunctionPointerLoaderTwoDefaultsHaveEqualHashCodes()
    {
        var left = default(AssemblyFunctionPointerLoader);
        var right = default(AssemblyFunctionPointerLoader);

        left.GetHashCode().ShouldBe(right.GetHashCode());
        left.GetHashCode().ShouldBe(nint.Zero.GetHashCode());
    }

    [Fact]
    public void AssemblyFunctionPointerLoaderDefaultEqualsBoxedDefault()
    {
        var loader = default(AssemblyFunctionPointerLoader);
        object boxed = default(AssemblyFunctionPointerLoader);

        loader.Equals(boxed).ShouldBeTrue();
    }

    [Fact]
    public void AssemblyFunctionPointerLoaderDefaultDoesNotEqualNullOrOtherType()
    {
        var loader = default(AssemblyFunctionPointerLoader);

        loader.Equals(null).ShouldBeFalse();
        loader.Equals("not a loader").ShouldBeFalse();
    }

    [Fact]
    public void AssemblyFunctionPointerLoaderLoadOnDefaultThrowsInvalidOperation()
    {
        Should.Throw<InvalidOperationException>(static () => default(AssemblyFunctionPointerLoader).Load("component.dll", "Some.Namespace.Type", "Method"));
    }

    [Theory]
    [InlineData(null, "Some.Namespace.Type", "Method")]
    [InlineData("", "Some.Namespace.Type", "Method")]
    [InlineData("component.dll", null, "Method")]
    [InlineData("component.dll", "", "Method")]
    [InlineData("component.dll", "Some.Namespace.Type", null)]
    [InlineData("component.dll", "Some.Namespace.Type", "")]
    public void AssemblyFunctionPointerLoaderLoadWithNullOrEmptyArgumentThrowsArgument(
        string? assemblyPath,
        string? typeName,
        string? methodName)
    {
        Should.Throw<ArgumentException>(() => default(AssemblyFunctionPointerLoader).Load(assemblyPath!, typeName!, methodName!));
    }

    // -------------------------------------------------------------------------
    // FunctionPointerResolver
    // -------------------------------------------------------------------------

    [Fact]
    public void FunctionPointerResolverDefaultIsNotValid()
    {
        var resolver = default(FunctionPointerResolver);

        resolver.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void FunctionPointerResolverDefaultUnderlyingPointerIsZero()
    {
        var resolver = default(FunctionPointerResolver);

        resolver.UnderlyingPointer.ShouldBe(nint.Zero);
    }

    [Fact]
    public void FunctionPointerResolverTwoDefaultsAreEqual()
    {
        var left = default(FunctionPointerResolver);
        var right = default(FunctionPointerResolver);

        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
    }

    [Fact]
    public void FunctionPointerResolverTwoDefaultsHaveEqualHashCodes()
    {
        var left = default(FunctionPointerResolver);
        var right = default(FunctionPointerResolver);

        left.GetHashCode().ShouldBe(right.GetHashCode());
        left.GetHashCode().ShouldBe(nint.Zero.GetHashCode());
    }

    [Fact]
    public void FunctionPointerResolverDefaultEqualsBoxedDefault()
    {
        var resolver = default(FunctionPointerResolver);
        object boxed = default(FunctionPointerResolver);

        resolver.Equals(boxed).ShouldBeTrue();
    }

    [Fact]
    public void FunctionPointerResolverDefaultDoesNotEqualNullOrOtherType()
    {
        var resolver = default(FunctionPointerResolver);

        resolver.Equals(null).ShouldBeFalse();
        resolver.Equals("not a resolver").ShouldBeFalse();
    }

    [Fact]
    public void FunctionPointerResolverResolveOnDefaultThrowsInvalidOperation()
    {
        Should.Throw<InvalidOperationException>(
            () => default(FunctionPointerResolver).Resolve("Some.Namespace.Type", "Method"));
    }

    [Theory]
    [InlineData(null, "Method")]
    [InlineData("", "Method")]
    [InlineData("Some.Namespace.Type", null)]
    [InlineData("Some.Namespace.Type", "")]
    public void FunctionPointerResolverResolveWithNullOrEmptyArgumentThrowsArgument(
        string? typeName,
        string? methodName)
    {
        Should.Throw<ArgumentException>(() => default(FunctionPointerResolver).Resolve(typeName!, methodName!));
    }

    // -------------------------------------------------------------------------
    // AssemblyBytesLoader
    // -------------------------------------------------------------------------

    [Fact]
    public void AssemblyBytesLoaderDefaultIsNotValid()
    {
        var loader = default(AssemblyBytesLoader);

        loader.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void AssemblyBytesLoaderDefaultUnderlyingPointerIsZero()
    {
        var loader = default(AssemblyBytesLoader);

        loader.UnderlyingPointer.ShouldBe(nint.Zero);
    }

    [Fact]
    public void AssemblyBytesLoaderTwoDefaultsAreEqual()
    {
        var left = default(AssemblyBytesLoader);
        var right = default(AssemblyBytesLoader);

        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
    }

    [Fact]
    public void AssemblyBytesLoaderTwoDefaultsHaveEqualHashCodes()
    {
        var left = default(AssemblyBytesLoader);
        var right = default(AssemblyBytesLoader);

        left.GetHashCode().ShouldBe(right.GetHashCode());
        left.GetHashCode().ShouldBe(nint.Zero.GetHashCode());
    }

    [Fact]
    public void AssemblyBytesLoaderDefaultEqualsBoxedDefault()
    {
        var loader = default(AssemblyBytesLoader);
        object boxed = default(AssemblyBytesLoader);

        loader.Equals(boxed).ShouldBeTrue();
    }

    [Fact]
    public void AssemblyBytesLoaderDefaultDoesNotEqualNullOrOtherType()
    {
        var loader = default(AssemblyBytesLoader);

        loader.Equals(null).ShouldBeFalse();
        loader.Equals("not a loader").ShouldBeFalse();
    }

    [Fact]
    public void AssemblyBytesLoaderLoadWithEmptySpanThrowsArgument()
    {
        Should.Throw<ArgumentException>(
            () => default(AssemblyBytesLoader).Load(ReadOnlySpan<byte>.Empty));
    }

    [Fact]
    public void AssemblyBytesLoaderLoadWithEmptyArrayThrowsArgument()
    {
        Should.Throw<ArgumentException>(static () => default(AssemblyBytesLoader).Load([]));
    }

    [Fact]
    public void AssemblyBytesLoaderLoadWithNonEmptyBytesOnDefaultThrowsInvalidOperation()
    {
        byte[] assemblyBytes = [1, 2, 3];

        Should.Throw<InvalidOperationException>(
            () => default(AssemblyBytesLoader).Load(assemblyBytes));
    }

    [Fact]
    public void AssemblyBytesLoaderLoadWithNonEmptyBytesAndSymbolsOnDefaultThrowsInvalidOperation()
    {
        byte[] assemblyBytes = [1, 2, 3];
        byte[] symbolBytes = [4, 5, 6];

        Should.Throw<InvalidOperationException>(() => default(AssemblyBytesLoader).Load(assemblyBytes, symbolBytes));
    }
}
