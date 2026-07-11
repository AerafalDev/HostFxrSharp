using HostFxrSharp.Loading;
using Shouldly;
using Xunit;

namespace HostFxrSharp.Tests;

/// <summary>
/// Unit tests for the <see cref="MethodSignature"/> value type: its factory members, value semantics
/// and the internal <c>ResolveNativePointer</c> mapping to native <c>delegate_type_name</c> values.
/// </summary>
public sealed class MethodSignatureTests
{
    [Fact]
    public void DefaultKeywordEqualsDefaultProperty()
    {
        MethodSignature fromKeyword = default;

        fromKeyword.ShouldBe(MethodSignature.Default);
        (fromKeyword == MethodSignature.Default).ShouldBeTrue();
    }

    [Fact]
    public void DefaultHasComponentEntryPointKindAndNullDelegateTypeName()
    {
        var signature = MethodSignature.Default;

        signature.Kind.ShouldBe(MethodSignatureKind.ComponentEntryPoint);
        signature.DelegateTypeName.ShouldBeNull();
    }

    [Fact]
    public void DefaultKeywordHasComponentEntryPointKindAndNullDelegateTypeName()
    {
        MethodSignature signature = default;

        signature.Kind.ShouldBe(MethodSignatureKind.ComponentEntryPoint);
        signature.DelegateTypeName.ShouldBeNull();
    }

    [Fact]
    public void UnmanagedCallersOnlyHasUnmanagedCallersOnlyKindAndNullDelegateTypeName()
    {
        var signature = MethodSignature.UnmanagedCallersOnly;

        signature.Kind.ShouldBe(MethodSignatureKind.UnmanagedCallersOnly);
        signature.DelegateTypeName.ShouldBeNull();
    }

    [Fact]
    public void FromDelegateTypeSetsDelegateTypeKind()
    {
        var signature = MethodSignature.FromDelegateType("MyApp.MyDelegate, MyApp");

        signature.Kind.ShouldBe(MethodSignatureKind.DelegateType);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("MyApp.MyDelegate, MyApp")]
    [InlineData("System.Action, System.Private.CoreLib")]
    public void FromDelegateTypeStoresSuppliedName(string name)
    {
        var signature = MethodSignature.FromDelegateType(name);

        signature.DelegateTypeName.ShouldBe(name);
        signature.Kind.ShouldBe(MethodSignatureKind.DelegateType);
    }

    [Fact]
    public void FromDelegateTypeWithNullThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(static () => MethodSignature.FromDelegateType(null!));
    }

    [Fact]
    public void FromDelegateTypeWithEmptyStringThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(static () => MethodSignature.FromDelegateType(string.Empty));
    }

    [Fact]
    public void EqualSignaturesOfSameKindAreEqual()
    {
        var first = MethodSignature.Default;
        var second = MethodSignature.Default;

        first.Equals(second).ShouldBeTrue();
        (first == second).ShouldBeTrue();
        (first != second).ShouldBeFalse();
    }

    [Fact]
    public void FromDelegateTypeWithSameNameAreEqual()
    {
        var first = MethodSignature.FromDelegateType("MyApp.MyDelegate, MyApp");
        var second = MethodSignature.FromDelegateType("MyApp.MyDelegate, MyApp");

        first.Equals(second).ShouldBeTrue();
        (first == second).ShouldBeTrue();
        (first != second).ShouldBeFalse();
    }

    [Fact]
    public void FromDelegateTypeWithDifferentNamesAreNotEqual()
    {
        var first = MethodSignature.FromDelegateType("MyApp.DelegateA, MyApp");
        var second = MethodSignature.FromDelegateType("MyApp.DelegateB, MyApp");

        first.Equals(second).ShouldBeFalse();
        (first == second).ShouldBeFalse();
        (first != second).ShouldBeTrue();
    }

    [Fact]
    public void FromDelegateTypeComparisonIsOrdinalCaseSensitive()
    {
        var lower = MethodSignature.FromDelegateType("myapp.mydelegate, myapp");
        var upper = MethodSignature.FromDelegateType("MYAPP.MYDELEGATE, MYAPP");

        lower.Equals(upper).ShouldBeFalse();
        (lower == upper).ShouldBeFalse();
        (lower != upper).ShouldBeTrue();
    }

    [Fact]
    public void DifferentKindsAreNotEqual()
    {
        var component = MethodSignature.Default;
        var unmanaged = MethodSignature.UnmanagedCallersOnly;
        var delegateType = MethodSignature.FromDelegateType("MyApp.MyDelegate, MyApp");

        (component == unmanaged).ShouldBeFalse();
        (component == delegateType).ShouldBeFalse();
        (unmanaged == delegateType).ShouldBeFalse();

        (component != unmanaged).ShouldBeTrue();
        (component != delegateType).ShouldBeTrue();
        (unmanaged != delegateType).ShouldBeTrue();
    }

    [Fact]
    public void EqualsObjectOverloadHandlesEqualValueOtherTypeAndNull()
    {
        var signature = MethodSignature.Default;

        signature.Equals((object)MethodSignature.Default).ShouldBeTrue();
        signature.Equals((object)MethodSignature.UnmanagedCallersOnly).ShouldBeFalse();
        signature.Equals("not a signature").ShouldBeFalse();
        signature.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCodeIsStableForEqualValues()
    {
        var firstComponent = MethodSignature.Default;
        var secondComponent = MethodSignature.Default;
        firstComponent.GetHashCode().ShouldBe(secondComponent.GetHashCode());

        var firstDelegate = MethodSignature.FromDelegateType("MyApp.MyDelegate, MyApp");
        var secondDelegate = MethodSignature.FromDelegateType("MyApp.MyDelegate, MyApp");
        firstDelegate.GetHashCode().ShouldBe(secondDelegate.GetHashCode());
    }

    [Fact]
    public unsafe void ResolveNativePointerReturnsNullForComponentEntryPoint()
    {
        var buffer = stackalloc byte[1];

        var resolved = MethodSignature.Default.ResolveNativePointer(buffer);

        ((IntPtr)resolved).ShouldBe(IntPtr.Zero);
    }

    [Fact]
    public unsafe void ResolveNativePointerReturnsSentinelForUnmanagedCallersOnly()
    {
        var buffer = stackalloc byte[1];

        var resolved = MethodSignature.UnmanagedCallersOnly.ResolveNativePointer(buffer);

        ((IntPtr)resolved).ShouldBe(new IntPtr(-1));
    }

    [Fact]
    public unsafe void ResolveNativePointerReturnsSuppliedPointerForDelegateType()
    {
        var buffer = stackalloc byte[1];
        var signature = MethodSignature.FromDelegateType("MyApp.MyDelegate, MyApp");

        var resolved = signature.ResolveNativePointer(buffer);

        ((IntPtr)resolved).ShouldBe((IntPtr)buffer);
    }
}
