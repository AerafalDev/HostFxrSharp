using Shouldly;
using Xunit;

namespace HostFxrSharp.Tests;

/// <summary>
/// Unit tests for the <see cref="HostStatusCode"/> enum and its classification helpers in
/// <see cref="HostingStatusCodeExtensions"/>. These tests are fully deterministic: they never load
/// the native hosting components and run identically on Windows, Linux and macOS.
/// </summary>
public sealed class HostStatusCodeTests
{
    /// <summary>
    /// Supplies every declared <see cref="HostStatusCode"/> member as an individual theory case.
    /// </summary>
    /// <returns>One <see cref="object"/> array per enum value.</returns>
    public static IEnumerable<object[]> AllStatusCodes()
    {
        return Enum.GetValues<HostStatusCode>().Select(static code => new object[] { code });
    }

    [Fact]
    public void EnumDefinesStatusCodes()
    {
        Enum.GetValues<HostStatusCode>().ShouldNotBeEmpty();
    }

    [Fact]
    public void AllStatusCodesHaveDistinctNumericValues()
    {
        var codes = Enum.GetValues<HostStatusCode>();

        codes.Select(static code => (int)code).Distinct().Count().ShouldBe(codes.Length);
    }

    [Fact]
    public void SuccessCodesHaveExpectedNumericValues()
    {
        ((int)HostStatusCode.Success).ShouldBe(0);
        ((int)HostStatusCode.SuccessHostAlreadyInitialized).ShouldBe(1);
        ((int)HostStatusCode.SuccessDifferentRuntimeProperties).ShouldBe(2);
    }

    [Fact]
    public void FailureCodesHaveExpectedNumericValues()
    {
        ((int)HostStatusCode.InvalidArgFailure).ShouldBe(unchecked((int)0x80008081));
        ((int)HostStatusCode.CoreHostLibLoadFailure).ShouldBe(unchecked((int)0x80008082));
        ((int)HostStatusCode.CoreHostLibMissingFailure).ShouldBe(unchecked((int)0x80008083));
        ((int)HostStatusCode.FrameworkMissingFailure).ShouldBe(unchecked((int)0x80008096));
        ((int)HostStatusCode.HostApiBufferTooSmall).ShouldBe(unchecked((int)0x80008098));
        ((int)HostStatusCode.HostApiUnsupportedVersion).ShouldBe(unchecked((int)0x800080a2));
        ((int)HostStatusCode.HostPropertyNotFound).ShouldBe(unchecked((int)0x800080a4));
        ((int)HostStatusCode.HostApiUnsupportedScenario).ShouldBe(unchecked((int)0x800080a6));
        ((int)HostStatusCode.HostFeatureDisabled).ShouldBe(unchecked((int)0x800080a7));
    }

    [Theory]
    [MemberData(nameof(AllStatusCodes))]
    public void IsSuccessMatchesSignOfUnderlyingValue(HostStatusCode code)
    {
        code.IsSuccess().ShouldBe((int)code >= 0);
    }

    [Theory]
    [MemberData(nameof(AllStatusCodes))]
    public void IsFailureMatchesSignOfUnderlyingValue(HostStatusCode code)
    {
        code.IsFailure().ShouldBe((int)code < 0);
    }

    [Theory]
    [MemberData(nameof(AllStatusCodes))]
    public void IsSuccessAndIsFailureAreMutuallyExclusive(HostStatusCode code)
    {
        // Exactly one of the two classifications must hold for every possible code.
        code.IsSuccess().ShouldNotBe(code.IsFailure());
    }

    [Theory]
    [MemberData(nameof(AllStatusCodes))]
    public void FailureCodesHaveHighBitSet(HostStatusCode code)
    {
        if (code.IsFailure())
        {
            (unchecked((uint)(int)code) & 0x8000_0000u).ShouldBe(0x8000_0000u);
        }
        else
        {
            ((int)code).ShouldBeGreaterThanOrEqualTo(0);
        }
    }

    [Theory]
    [MemberData(nameof(AllStatusCodes))]
    public void IsSecondaryContextIsTrueOnlyForSecondarySuccessCodes(HostStatusCode code)
    {
        var expected = code is HostStatusCode.SuccessHostAlreadyInitialized
            or HostStatusCode.SuccessDifferentRuntimeProperties;

        code.IsSecondaryContext().ShouldBe(expected);
    }

    [Fact]
    public void SecondaryContextCodesAreAlsoSuccesses()
    {
        HostStatusCode.SuccessHostAlreadyInitialized.IsSuccess().ShouldBeTrue();
        HostStatusCode.SuccessHostAlreadyInitialized.IsSecondaryContext().ShouldBeTrue();

        HostStatusCode.SuccessDifferentRuntimeProperties.IsSuccess().ShouldBeTrue();
        HostStatusCode.SuccessDifferentRuntimeProperties.IsSecondaryContext().ShouldBeTrue();
    }

    [Theory]
    [InlineData(HostStatusCode.Success)]
    [InlineData(HostStatusCode.SuccessHostAlreadyInitialized)]
    [InlineData(HostStatusCode.SuccessDifferentRuntimeProperties)]
    public void PrimarySuccessAndWarningCodesReportSuccess(HostStatusCode code)
    {
        code.IsSuccess().ShouldBeTrue();
        code.IsFailure().ShouldBeFalse();
    }

    [Theory]
    [InlineData(HostStatusCode.Success)]
    [InlineData(HostStatusCode.InvalidArgFailure)]
    [InlineData(HostStatusCode.HostApiBufferTooSmall)]
    [InlineData(HostStatusCode.HostPropertyNotFound)]
    [InlineData(HostStatusCode.FrameworkMissingFailure)]
    public void NonSecondaryCodesAreNotSecondaryContext(HostStatusCode code)
    {
        code.IsSecondaryContext().ShouldBeFalse();
    }
}
