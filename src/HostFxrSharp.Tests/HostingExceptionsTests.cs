using HostFxrSharp.Exceptions;
using Shouldly;
using Xunit;

namespace HostFxrSharp.Tests;

/// <summary>
/// Unit tests for the <see cref="HostingException"/> hierarchy: construction, message / inner-exception /
/// status-code propagation, the real inheritance relationships and the
/// <see cref="RuntimePropertyNotFoundException.ForProperty"/> factory.
/// </summary>
public sealed class HostingExceptionsTests
{
    private static readonly Type[] StatusCodeConstructorSignature = [typeof(string), typeof(HostStatusCode)];

    /// <summary>All concrete exception types in the hosting hierarchy.</summary>
    public static TheoryData<Type> AllExceptionTypes =>
        new(typeof(HostingApiException),
            typeof(HostFxrResolutionException),
            typeof(HostFxrNotFoundException),
            typeof(IncompatibleHostPolicyException),
            typeof(UnsupportedHostingScenarioException),
            typeof(RuntimeAlreadyLoadedException),
            typeof(RuntimePropertyNotFoundException),
            typeof(HostInvalidStateException));

    /// <summary>Concrete types that expose a <c>(string message, HostStatusCode statusCode)</c> constructor.</summary>
    public static TheoryData<Type> TypesWithStatusCodeConstructor =>
        new(typeof(HostingApiException),
            typeof(HostFxrResolutionException),
            typeof(IncompatibleHostPolicyException),
            typeof(UnsupportedHostingScenarioException),
            typeof(RuntimeAlreadyLoadedException),
            typeof(HostInvalidStateException));

    /// <summary>Types that derive (transitively) from <see cref="HostingApiException"/>.</summary>
    public static TheoryData<Type> HostingApiSubtypes =>
        new(typeof(IncompatibleHostPolicyException),
            typeof(UnsupportedHostingScenarioException),
            typeof(RuntimeAlreadyLoadedException),
            typeof(RuntimePropertyNotFoundException),
            typeof(HostInvalidStateException));

    /// <summary>Direct <see cref="HostingException"/> subtypes that bypass <see cref="HostingApiException"/>.</summary>
    public static TheoryData<Type> DirectHostingExceptionSubtypes =>
        new(typeof(HostFxrResolutionException),
            typeof(HostFxrNotFoundException));

    // ----------------------------------------------------------------------------------------------
    // Inheritance relationships
    // ----------------------------------------------------------------------------------------------

    [Fact]
    public void HostingExceptionIsAbstractAndDerivesFromException()
    {
        typeof(HostingException).IsAbstract.ShouldBeTrue();
        typeof(HostingException).IsSubclassOf(typeof(Exception)).ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(AllExceptionTypes))]
    public void EveryExceptionDerivesFromHostingException(Type type)
    {
        type.IsSubclassOf(typeof(HostingException)).ShouldBeTrue();
        typeof(Exception).IsAssignableFrom(type).ShouldBeTrue();
    }

    [Fact]
    public void HostingApiExceptionDerivesDirectlyFromHostingException()
    {
        typeof(HostingApiException).BaseType.ShouldBe(typeof(HostingException));
    }

    [Fact]
    public void HostFxrResolutionExceptionDerivesDirectlyFromHostingException()
    {
        typeof(HostFxrResolutionException).BaseType.ShouldBe(typeof(HostingException));
    }

    [Fact]
    public void HostFxrNotFoundExceptionDerivesDirectlyFromHostingException()
    {
        typeof(HostFxrNotFoundException).BaseType.ShouldBe(typeof(HostingException));
    }

    [Theory]
    [MemberData(nameof(HostingApiSubtypes))]
    public void ApiSubtypesDeriveFromHostingApiException(Type type)
    {
        type.IsSubclassOf(typeof(HostingApiException)).ShouldBeTrue();
        typeof(HostingApiException).BaseType.ShouldBe(typeof(HostingException));
    }

    [Theory]
    [MemberData(nameof(DirectHostingExceptionSubtypes))]
    public void DirectSubtypesDoNotDeriveFromHostingApiException(Type type)
    {
        type.IsSubclassOf(typeof(HostingApiException)).ShouldBeFalse();
        type.IsSubclassOf(typeof(HostingException)).ShouldBeTrue();
    }

    // ----------------------------------------------------------------------------------------------
    // Constructors: message / inner exception / status code
    // ----------------------------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllExceptionTypes))]
    public void DefaultConstructorLeavesStatusCodeAndInnerNull(Type type)
    {
        var ex = (HostingException)Activator.CreateInstance(type)!;

        ex.StatusCode.ShouldBeNull();
        ex.InnerException.ShouldBeNull();
        ex.Message.ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [MemberData(nameof(AllExceptionTypes))]
    public void MessageConstructorPreservesMessage(Type type)
    {
        const string message = "something went wrong while hosting the runtime";

        var ex = (HostingException)Activator.CreateInstance(type, message)!;

        ex.Message.ShouldBe(message);
        ex.StatusCode.ShouldBeNull();
        ex.InnerException.ShouldBeNull();
    }

    [Theory]
    [MemberData(nameof(AllExceptionTypes))]
    public void InnerExceptionConstructorPreservesMessageAndInner(Type type)
    {
        const string message = "wrapping a lower-level failure";
        InvalidOperationException inner = new("root cause");

        var ex = (HostingException)Activator.CreateInstance(type, message, inner)!;

        ex.Message.ShouldBe(message);
        ex.InnerException.ShouldBeSameAs(inner);
        ex.StatusCode.ShouldBeNull();
    }

    [Theory]
    [MemberData(nameof(TypesWithStatusCodeConstructor))]
    public void StatusCodeConstructorPreservesMessageAndStatusCode(Type type)
    {
        const string message = "the native hosting call failed";

        var ex = (HostingException)Activator.CreateInstance(type, message, HostStatusCode.HostApiFailed)!;

        ex.Message.ShouldBe(message);
        ex.StatusCode.ShouldBe(HostStatusCode.HostApiFailed);
        ex.InnerException.ShouldBeNull();
    }

    [Theory]
    [InlineData(HostStatusCode.Success)]
    [InlineData(HostStatusCode.HostApiFailed)]
    [InlineData(HostStatusCode.HostApiUnsupportedVersion)]
    [InlineData(HostStatusCode.HostInvalidState)]
    [InlineData(HostStatusCode.HostPropertyNotFound)]
    [InlineData(HostStatusCode.FrameworkMissingFailure)]
    public void StatusCodeConstructorStoresProvidedCode(HostStatusCode statusCode)
    {
        HostingApiException ex = new("native failure", statusCode);

        ex.StatusCode.ShouldBe(statusCode);
        ex.Message.ShouldBe("native failure");
    }

    // ----------------------------------------------------------------------------------------------
    // Constructor surface: which types carry the status-code overload
    // ----------------------------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(TypesWithStatusCodeConstructor))]
    public void TypesWithStatusCodeConstructorExposeIt(Type type)
    {
        type.GetConstructor(StatusCodeConstructorSignature).ShouldNotBeNull();
    }

    [Theory]
    [InlineData(typeof(HostFxrNotFoundException))]
    [InlineData(typeof(RuntimePropertyNotFoundException))]
    public void TypesWithoutStatusCodeConstructorDoNotExposeOne(Type type)
    {
        type.GetConstructor(StatusCodeConstructorSignature).ShouldBeNull();
    }

    // ----------------------------------------------------------------------------------------------
    // Throw / catch behaviour
    // ----------------------------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllExceptionTypes))]
    public void EveryExceptionCanBeCaughtAsHostingException(Type type)
    {
        var instance = (HostingException)Activator.CreateInstance(type, "boom")!;

        var caught = Should.Throw<HostingException>(() => throw instance);

        caught.ShouldBeSameAs(instance);
        caught.Message.ShouldBe("boom");
    }

    [Theory]
    [MemberData(nameof(HostingApiSubtypes))]
    public void ApiSubtypesCanBeCaughtAsHostingApiException(Type type)
    {
        var instance = (HostingApiException)Activator.CreateInstance(type, "api boom")!;

        var caught = Should.Throw<HostingApiException>(() => throw instance);

        caught.ShouldBeSameAs(instance);
    }

    // ----------------------------------------------------------------------------------------------
    // Type-specific behaviour
    // ----------------------------------------------------------------------------------------------

    [Fact]
    public void HostFxrNotFoundExceptionDefaultMessageMentionsHostFxr()
    {
        HostFxrNotFoundException ex = new();

        ex.Message.ShouldContain("hostfxr");
        ex.StatusCode.ShouldBeNull();
        ex.InnerException.ShouldBeNull();
    }

    [Theory]
    [InlineData("APP_CONTEXT_BASE_DIRECTORY")]
    [InlineData("TRUSTED_PLATFORM_ASSEMBLIES")]
    [InlineData("some.custom.property")]
    public void ForPropertyIncludesPropertyNameInMessageAndProperty(string propertyName)
    {
        var ex = RuntimePropertyNotFoundException.ForProperty(propertyName);

        ex.Message.ShouldContain(propertyName);
        ex.PropertyName.ShouldBe(propertyName);
        ex.StatusCode.ShouldBeNull();
    }

    [Fact]
    public void ForPropertyReturnsHostingApiException()
    {
        var ex = RuntimePropertyNotFoundException.ForProperty("STARTUP_HOOKS");

        ex.ShouldBeAssignableTo<HostingApiException>();
    }

    [Fact]
    public void RuntimePropertyNotFoundExceptionPropertyNameDefaultsToNull()
    {
        RuntimePropertyNotFoundException ex = new("a runtime property was missing");

        ex.PropertyName.ShouldBeNull();
    }

    [Fact]
    public void RuntimePropertyNotFoundExceptionPropertyNameCanBeInitialized()
    {
        RuntimePropertyNotFoundException ex = new("missing property")
        {
            PropertyName = "TRUSTED_PLATFORM_ASSEMBLIES",
        };

        ex.PropertyName.ShouldBe("TRUSTED_PLATFORM_ASSEMBLIES");
        ex.Message.ShouldBe("missing property");
    }
}
