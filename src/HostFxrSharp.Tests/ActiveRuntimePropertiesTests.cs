using HostFxrSharp.Exceptions;
using Shouldly;
using Xunit;

namespace HostFxrSharp.Tests;

/// <summary>
/// Integration tests over the process-wide active host context. Reading the active context requires the current
/// process to have started through <c>hostfxr</c>, so every host-dependent test is guarded and skips (rather than
/// fails) when the environment cannot supply the expected host bits. Beyond exercising the API surface, these tests
/// double as a strong, cross-platform validation that the native <c>char_t</c> strings surfaced by <c>hostfxr</c>
/// are decoded into correct managed strings.
/// </summary>
public sealed class ActiveRuntimePropertiesTests
{
    private const string TrustedPlatformAssemblies = "TRUSTED_PLATFORM_ASSEMBLIES";

    private const string ObviouslyAbsentProperty = "HOSTFXRSHARP_DEFINITELY_ABSENT_PROPERTY_9F3C1A7E";

    [SkippableFact]
    public void GetActiveRuntimePropertiesReturnsNonEmptyDictionary()
    {
        try
        {
            HostFxr.EnsureLoaded();

            var properties = HostFxr.GetActiveRuntimeProperties();

            properties.ShouldNotBeNull();
            properties.Count.ShouldBeGreaterThan(0);
        }
        catch (HostingException ex)
        {
            throw new SkipException($"Active runtime properties are unavailable in this environment: {ex.Message}");
        }
    }

    [SkippableFact]
    public void AllActivePropertyKeysAndValuesDecodeCleanly()
    {
        try
        {
            HostFxr.EnsureLoaded();

            var properties = HostFxr.GetActiveRuntimeProperties();
            properties.Count.ShouldBeGreaterThan(0);

            foreach (var pair in properties)
            {
                // A correctly decoded char_t string is never null, never empty for a key, never contains an
                // interior NUL, and never contains the U+FFFD replacement character produced by a botched decode.
                pair.Key.ShouldNotBeNullOrEmpty();
                pair.Key.ShouldNotContain('\0');
                pair.Key.ShouldNotContain('�');

                pair.Value.ShouldNotBeNull();
                pair.Value.ShouldNotContain('\0');
                pair.Value.ShouldNotContain('�');
            }
        }
        catch (HostingException ex)
        {
            throw new SkipException($"Active runtime properties are unavailable in this environment: {ex.Message}");
        }
    }

    [SkippableFact]
    public void TrustedPlatformAssembliesDecodesToLongSemicolonSeparatedList()
    {
        try
        {
            HostFxr.EnsureLoaded();

            // TPA is a long list of assembly paths joined by the platform path separator (';' on Windows, ':' on
            // Unix). A correct char_t decode reproduces every separator and path intact, so this is a strong
            // end-to-end decode check on a large real-world string.
            var found = HostFxr.TryGetActiveRuntimeProperty(TrustedPlatformAssemblies, out var tpa);

            found.ShouldBeTrue();
            tpa.ShouldNotBeNull();

            tpa.ShouldNotBeEmpty();
            tpa.ShouldContain(Path.PathSeparator);
            tpa.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).Length.ShouldBeGreaterThan(1);
            tpa.ShouldNotContain('\0');
            tpa.ShouldNotContain('�');
        }
        catch (HostingException ex)
        {
            throw new SkipException($"Active runtime properties are unavailable in this environment: {ex.Message}");
        }
    }

    [SkippableFact]
    public void TryGetActiveRuntimePropertyReturnsFalseForAbsentProperty()
    {
        try
        {
            HostFxr.EnsureLoaded();

            var found = HostFxr.TryGetActiveRuntimeProperty(ObviouslyAbsentProperty, out var value);

            found.ShouldBeFalse();
            value.ShouldBeNull();
        }
        catch (HostingException ex)
        {
            throw new SkipException($"Active runtime properties are unavailable in this environment: {ex.Message}");
        }
    }

    [SkippableFact]
    public void TryGetActiveRuntimePropertyAgreesWithSnapshotForEveryKey()
    {
        try
        {
            HostFxr.EnsureLoaded();

            var properties = HostFxr.GetActiveRuntimeProperties();
            properties.Count.ShouldBeGreaterThan(0);

            // The bulk snapshot (hostfxr_get_runtime_properties) and the single-property lookup
            // (hostfxr_get_runtime_property_value) are independent native paths; requiring them to agree on every
            // key exercises char_t decoding across many real strings from two directions at once.
            foreach (var pair in properties)
            {
                var found = HostFxr.TryGetActiveRuntimeProperty(pair.Key, out var value);

                found.ShouldBeTrue();
                value.ShouldBe(pair.Value);
            }
        }
        catch (HostingException ex)
        {
            throw new SkipException($"Active runtime properties are unavailable in this environment: {ex.Message}");
        }
    }

    [SkippableTheory]
    [InlineData("TRUSTED_PLATFORM_ASSEMBLIES")]
    [InlineData("NATIVE_DLL_SEARCH_DIRECTORIES")]
    [InlineData("PLATFORM_RESOURCE_ROOTS")]
    [InlineData("APP_CONTEXT_BASE_DIRECTORY")]
    [InlineData("APP_CONTEXT_DEPS_FILES")]
    [InlineData("FX_DEPS_FILE")]
    [InlineData("RUNTIME_IDENTIFIER")]
    public void KnownRuntimePropertyWhenPresentDecodesCleanly(string name)
    {
        try
        {
            HostFxr.EnsureLoaded();

            if (!HostFxr.TryGetActiveRuntimeProperty(name, out var value))
                throw new SkipException($"Runtime property '{name}' is not present in this environment.");

            value.ShouldNotContain('\0');
            value.ShouldNotContain('�');
        }
        catch (HostingException ex)
        {
            throw new SkipException($"Active runtime properties are unavailable in this environment: {ex.Message}");
        }
    }

    [SkippableTheory]
    [InlineData(null)]
    [InlineData("")]
    public void TryGetActiveRuntimePropertyRejectsNullOrEmptyName(string? name)
    {
        // Argument validation runs before any native call, so this is deterministic and needs no host guard.
        Should.Throw<ArgumentException>(() => HostFxr.TryGetActiveRuntimeProperty(name!, out _));
    }
}
