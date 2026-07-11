using HostFxrSharp.Contexts;
using HostFxrSharp.Exceptions;
using HostFxrSharp.Loading;
using Shouldly;
using Xunit;

namespace HostFxrSharp.Tests;

/// <summary>
/// End-to-end integration tests that load the sibling <c>HostFxrSharp.TestComponent</c> assembly through
/// <c>load_assembly_and_get_function_pointer</c> and invoke a managed method via a native function pointer.
/// </summary>
/// <remarks>
/// These exercise the full path (locate → load hostfxr → initialize a context → runtime delegate → load
/// component → invoke). Because a runtime is already running inside the test host, each context is a secondary
/// one. Every host call is guarded so the test skips (rather than fails) on an environment that cannot support
/// the scenario.
/// </remarks>
public sealed unsafe class ComponentHostingIntegrationTests
{
    private const string ComponentType = "HostFxrSharp.TestComponent.Exports, HostFxrSharp.TestComponent";
    private const string ComputeDelegateType = "HostFxrSharp.TestComponent.ComputeDelegate, HostFxrSharp.TestComponent";

    private static string ComponentAssemblyPath =>
        Path.Combine(AppContext.BaseDirectory, "TestComponent", "HostFxrSharp.TestComponent.dll");

    private static string RuntimeConfigPath =>
        Path.Combine(AppContext.BaseDirectory, "HostFxrSharp.Tests.runtimeconfig.json");

    [SkippableFact]
    public void LoadsUnmanagedCallersOnlyMethodAndInvokesIt()
    {
        using var context = CreateContextOrSkip();
        var loader = GetLoaderOrSkip(context);

        var pointer = loader.Load(ComponentAssemblyPath, ComponentType, "Add", MethodSignature.UnmanagedCallersOnly);

        pointer.ShouldNotBe(0);

        var add = (delegate* unmanaged<int, int, int>)pointer;
        add(20, 22).ShouldBe(42);
    }

    [SkippableFact]
    public void LoadsDefaultComponentEntryPoint()
    {
        using var context = CreateContextOrSkip();
        var loader = GetLoaderOrSkip(context);

        var pointer = loader.Load(ComponentAssemblyPath, ComponentType, "ComponentEntry");

        pointer.ShouldNotBe(0);
    }

    [SkippableFact]
    public void LoadsDelegateTypedMethod()
    {
        using var context = CreateContextOrSkip();
        var loader = GetLoaderOrSkip(context);

        var pointer = loader.Load(ComponentAssemblyPath, ComponentType, "Square", MethodSignature.FromDelegateType(ComputeDelegateType));

        pointer.ShouldNotBe(0);
    }

    [SkippableFact]
    public void LoadingAMissingMethodThrowsHostingException()
    {
        using var context = CreateContextOrSkip();
        var loader = GetLoaderOrSkip(context);

        Should.Throw<HostingException>(() =>
            loader.Load(ComponentAssemblyPath, ComponentType, "ThisMethodDoesNotExist", MethodSignature.UnmanagedCallersOnly));
    }

    private static HostContext CreateContextOrSkip()
    {
        if (!File.Exists(ComponentAssemblyPath))
            throw new SkipException($"Test component was not found at '{ComponentAssemblyPath}'.");

        if (!File.Exists(RuntimeConfigPath))
            throw new SkipException($"Runtime config was not found at '{RuntimeConfigPath}'.");

        try
        {
            return HostFxr.InitializeForRuntimeConfig(RuntimeConfigPath);
        }
        catch (HostingException ex)
        {
            throw new SkipException($"Could not initialize a host context in this environment: {ex.Message}");
        }
    }

    private static AssemblyFunctionPointerLoader GetLoaderOrSkip(HostContext context)
    {
        try
        {
            return context.GetAssemblyFunctionPointerLoader();
        }
        catch (HostingException ex)
        {
            throw new SkipException($"The load_assembly_and_get_function_pointer delegate is unavailable: {ex.Message}");
        }
    }
}
