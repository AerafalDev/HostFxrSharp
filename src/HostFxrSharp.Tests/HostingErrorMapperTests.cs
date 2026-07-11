using HostFxrSharp.Exceptions;
using HostFxrSharp.Interop;
using Shouldly;
using Xunit;

namespace HostFxrSharp.Tests;

/// <summary>
/// Unit tests for <see cref="HostingErrorMapper"/>, covering every branch that maps a native
/// <see cref="HostStatusCode"/> onto a typed <see cref="HostingException"/>.
/// </summary>
public sealed class HostingErrorMapperTests
{
    private const string Operation = "hostfxr_initialize_for_runtime_config";

    /// <summary>All failure status codes (the high-bit <c>0x8xxxxxxx</c> range) declared by the enum.</summary>
    public static TheoryData<HostStatusCode> FailureStatusCodes()
    {
        var data = new TheoryData<HostStatusCode>();

        foreach (var code in Enum.GetValues<HostStatusCode>())
        {
            if ((int)code < 0)
                data.Add(code);
        }

        return data;
    }

    [Fact]
    public void HostApiUnsupportedVersionMapsToIncompatibleHostPolicyException()
    {
        var ex = HostingErrorMapper.ToException(HostStatusCode.HostApiUnsupportedVersion, Operation);

        ex.ShouldBeOfType<IncompatibleHostPolicyException>();
        ex.StatusCode.ShouldBe(HostStatusCode.HostApiUnsupportedVersion);
        ex.Message.ShouldContain($"'{Operation}'");
        ex.Message.ShouldContain("HostApiUnsupportedVersion");
        ex.Message.ShouldContain("0x800080A2");
        ex.Message.ShouldContain("hostpolicy");
    }

    [Fact]
    public void HostApiUnsupportedScenarioMapsToUnsupportedHostingScenarioException()
    {
        var ex = HostingErrorMapper.ToException(HostStatusCode.HostApiUnsupportedScenario, Operation);

        ex.ShouldBeOfType<UnsupportedHostingScenarioException>();
        ex.StatusCode.ShouldBe(HostStatusCode.HostApiUnsupportedScenario);
        ex.Message.ShouldContain($"'{Operation}'");
        ex.Message.ShouldContain("HostApiUnsupportedScenario");
        ex.Message.ShouldContain("0x800080A6");
        ex.Message.ShouldContain("not supported");
    }

    [Fact]
    public void HostPropertyNotFoundMapsToRuntimePropertyNotFoundExceptionWithoutStatusCode()
    {
        var ex = HostingErrorMapper.ToException(HostStatusCode.HostPropertyNotFound, Operation);

        var typed = ex.ShouldBeOfType<RuntimePropertyNotFoundException>();
        // The mapper constructs this one with the base message only, so no code or property name is attached.
        typed.PropertyName.ShouldBeNull();
        ex.StatusCode.ShouldBeNull();
        ex.Message.ShouldContain($"'{Operation}'");
        ex.Message.ShouldContain("HostPropertyNotFound");
        ex.Message.ShouldContain("0x800080A4");
    }

    [Fact]
    public void CoreHostLibMissingFailureMapsToHostFxrNotFoundException()
    {
        var ex = HostingErrorMapper.ToException(HostStatusCode.CoreHostLibMissingFailure, Operation);

        ex.ShouldBeOfType<HostFxrNotFoundException>();
        // HostFxrNotFoundException derives directly from HostingException and never carries a status code.
        ex.StatusCode.ShouldBeNull();
        (ex is HostingApiException).ShouldBeFalse();
        ex.Message.ShouldContain($"'{Operation}'");
        ex.Message.ShouldContain("CoreHostLibMissingFailure");
        ex.Message.ShouldContain("0x80008083");
        ex.Message.ShouldContain("could not be found");
    }

    [Fact]
    public void HostInvalidStateWithoutInitKindMapsToHostInvalidStateException()
    {
        var ex = HostingErrorMapper.ToException(HostStatusCode.HostInvalidState, Operation);

        ex.ShouldBeOfType<HostInvalidStateException>();
        ex.StatusCode.ShouldBe(HostStatusCode.HostInvalidState);
        ex.Message.ShouldContain($"'{Operation}'");
        ex.Message.ShouldContain("HostInvalidState");
        ex.Message.ShouldContain("0x800080A3");
    }

    [Fact]
    public void HostInvalidStateWithRuntimeConfigMapsToHostInvalidStateException()
    {
        var ex = HostingErrorMapper.ToException(HostStatusCode.HostInvalidState, Operation, HostContextInitKind.RuntimeConfig);

        ex.ShouldBeOfType<HostInvalidStateException>();
        ex.StatusCode.ShouldBe(HostStatusCode.HostInvalidState);
        ex.Message.ShouldContain($"'{Operation}'");
        ex.Message.ShouldContain("0x800080A3");
    }

    [Fact]
    public void HostInvalidStateWithCommandLineMapsToRuntimeAlreadyLoadedException()
    {
        var ex = HostingErrorMapper.ToException(HostStatusCode.HostInvalidState, Operation, HostContextInitKind.CommandLine);

        ex.ShouldBeOfType<RuntimeAlreadyLoadedException>();
        ex.StatusCode.ShouldBe(HostStatusCode.HostInvalidState);
        ex.Message.ShouldContain($"'{Operation}'");
        ex.Message.ShouldContain("HostInvalidState");
        ex.Message.ShouldContain("0x800080A3");
        ex.Message.ShouldContain("already loaded");
    }

    [Fact]
    public void InitKindOnlyChangesTheMappingForHostInvalidState()
    {
        // A non-null init kind must be ignored for any status code other than HostInvalidState.
        var ex = HostingErrorMapper.ToException(HostStatusCode.HostApiUnsupportedVersion, Operation, HostContextInitKind.CommandLine);

        ex.ShouldBeOfType<IncompatibleHostPolicyException>();
        ex.StatusCode.ShouldBe(HostStatusCode.HostApiUnsupportedVersion);
    }

    [Theory]
    [InlineData(HostStatusCode.InvalidArgFailure)]
    [InlineData(HostStatusCode.CoreHostLibLoadFailure)]
    [InlineData(HostStatusCode.CoreClrInitFailure)]
    [InlineData(HostStatusCode.FrameworkMissingFailure)]
    [InlineData(HostStatusCode.InvalidConfigFile)]
    [InlineData(HostStatusCode.HostApiFailed)]
    [InlineData(HostStatusCode.HostIncompatibleConfig)]
    [InlineData(HostStatusCode.HostFeatureDisabled)]
    [InlineData(HostStatusCode.LibHostDuplicateProperty)]
    [InlineData(HostStatusCode.BundleExtractionFailure)]
    public void UnmappedFailureCodesMapToPlainHostingApiException(HostStatusCode code)
    {
        var ex = HostingErrorMapper.ToException(code, Operation);

        // ShouldBeOfType asserts the exact runtime type, proving these hit the default switch arm
        // rather than one of the specialized subtypes.
        ex.ShouldBeOfType<HostingApiException>();
        ex.StatusCode.ShouldBe(code);
        ex.Message.ShouldContain($"'{Operation}'");
        ex.Message.ShouldContain(code.ToString());
        ex.Message.ShouldContain($"0x{(uint)code:X8}");
    }

    [Theory]
    [MemberData(nameof(FailureStatusCodes))]
    public void EveryFailureCodeProducesAHostingExceptionCarryingTheOperationAndHexCode(HostStatusCode code)
    {
        var ex = HostingErrorMapper.ToException(code, Operation);

        ex.ShouldNotBeNull();
        ex.Message.ShouldNotBeNullOrWhiteSpace();
        ex.Message.ShouldContain($"'{Operation}'");
        ex.Message.ShouldContain(code.ToString());
        ex.Message.ShouldContain($"0x{(uint)code:X8}");
    }

    [Theory]
    [InlineData("hostfxr_initialize_for_runtime_config")]
    [InlineData("hostfxr_get_runtime_delegate")]
    [InlineData("nethost_get_hostfxr_path")]
    public void MessageEmbedsTheSuppliedOperationName(string operation)
    {
        var ex = HostingErrorMapper.ToException(HostStatusCode.CoreClrInitFailure, operation);

        ex.Message.ShouldContain($"'{operation}'");
        ex.Message.ShouldContain("0x80008089");
    }

    [Fact]
    public void StatusCodeCarryingExceptionsDeriveFromHostingApiException()
    {
        HostingErrorMapper.ToException(HostStatusCode.HostApiUnsupportedVersion, Operation).ShouldBeAssignableTo<HostingApiException>();
        HostingErrorMapper.ToException(HostStatusCode.HostApiUnsupportedScenario, Operation).ShouldBeAssignableTo<HostingApiException>();
        HostingErrorMapper.ToException(HostStatusCode.HostPropertyNotFound, Operation).ShouldBeAssignableTo<HostingApiException>();
        HostingErrorMapper.ToException(HostStatusCode.HostInvalidState, Operation).ShouldBeAssignableTo<HostingApiException>();
        HostingErrorMapper.ToException(HostStatusCode.HostInvalidState, Operation, HostContextInitKind.CommandLine).ShouldBeAssignableTo<HostingApiException>();
    }
}
