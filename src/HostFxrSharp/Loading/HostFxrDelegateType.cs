using HostFxrSharp.Contexts;
using HostFxrSharp.Exceptions;

namespace HostFxrSharp.Loading;

/// <summary>
/// The values of the native <c>enum hostfxr_delegate_type</c> (from <c>hostfxr.h</c>), used with
/// <see cref="HostContext.GetRuntimeDelegate(HostFxrDelegateType)"/>.
/// </summary>
/// <remarks>
/// <para>
/// The numeric values and order match the native header <b>exactly</b> (verified in Phase&#160;0; see
/// <c>docs/RESEARCH.md §4</c>). They are implicit sequential values and must not be reordered.
/// </para>
/// <para>Availability by runtime version:</para>
/// <list type="table">
///   <listheader><term>Value</term><description>Availability</description></listheader>
///   <item><term><see cref="ComActivation"/> (0)</term><description>.NET Core 3.0+</description></item>
///   <item><term><see cref="LoadInMemoryAssembly"/> (1)</term><description>.NET Core 3.0+ (IJW)</description></item>
///   <item><term><see cref="WinRtActivation"/> (2)</term><description><b>.NET Core 3.x only</b> — removed in .NET 5+</description></item>
///   <item><term><see cref="ComRegister"/> (3)</term><description>.NET Core 3.0+</description></item>
///   <item><term><see cref="ComUnregister"/> (4)</term><description>.NET Core 3.0+</description></item>
///   <item><term><see cref="LoadAssemblyAndGetFunctionPointer"/> (5)</term><description>.NET Core 3.0+</description></item>
///   <item><term><see cref="GetFunctionPointer"/> (6)</term><description>.NET 5+</description></item>
///   <item><term><see cref="LoadAssembly"/> (7)</term><description>.NET 8+</description></item>
///   <item><term><see cref="LoadAssemblyBytes"/> (8)</term><description>.NET 8+</description></item>
/// </list>
/// </remarks>
public enum HostFxrDelegateType
{
    /// <summary>
    /// <c>hdt_com_activation</c> (0). COM activation entry point. Available in .NET Core 3.0+.
    /// Obtain the raw pointer via <see cref="HostContext.GetRuntimeDelegate(HostFxrDelegateType)"/> and cast to
    /// the signature documented in the runtime's COM-activation feature doc.
    /// </summary>
    ComActivation = 0,

    /// <summary>
    /// <c>hdt_load_in_memory_assembly</c> (1). IJW (mixed-mode) in-memory assembly load entry point.
    /// Available in .NET Core 3.0+.
    /// </summary>
    LoadInMemoryAssembly = 1,

    /// <summary>
    /// <c>hdt_winrt_activation</c> (2). WinRT activation entry point. <b>Available on .NET Core 3.x only and
    /// removed in .NET 5 and later.</b> Requesting it on a modern runtime throws
    /// <see cref="UnsupportedHostingScenarioException"/>. (The C spelling is one word: <c>winrt</c>.)
    /// </summary>
    WinRtActivation = 2,

    /// <summary><c>hdt_com_register</c> (3). COM registration entry point. Available in .NET Core 3.0+.</summary>
    ComRegister = 3,

    /// <summary><c>hdt_com_unregister</c> (4). COM unregistration entry point. Available in .NET Core 3.0+.</summary>
    ComUnregister = 4,

    /// <summary>
    /// <c>hdt_load_assembly_and_get_function_pointer</c> (5). Loads an assembly into an isolated
    /// <see cref="System.Runtime.Loader.AssemblyLoadContext"/> and returns a function pointer to a static method.
    /// Available in .NET Core 3.0+. Prefer <see cref="HostContext.GetAssemblyFunctionPointerLoader"/> for a typed wrapper.
    /// </summary>
    LoadAssemblyAndGetFunctionPointer = 5,

    /// <summary>
    /// <c>hdt_get_function_pointer</c> (6). Returns a function pointer for an already-available method in the
    /// default load context. Available in .NET 5+. Prefer <see cref="HostContext.GetFunctionPointerResolver"/>.
    /// </summary>
    GetFunctionPointer = 6,

    /// <summary>
    /// <c>hdt_load_assembly</c> (7). Loads an assembly by path into the default load context.
    /// Available in .NET 8+. Prefer <see cref="HostContext.GetAssemblyLoader"/>.
    /// </summary>
    LoadAssembly = 7,

    /// <summary>
    /// <c>hdt_load_assembly_bytes</c> (8). Loads an assembly from a byte array into the default load context.
    /// Available in .NET 8+. Prefer <see cref="HostContext.GetAssemblyBytesLoader"/>.
    /// </summary>
    LoadAssemblyBytes = 8
}
