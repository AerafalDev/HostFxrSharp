using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HostFxrSharp.Interop.Structs;

namespace HostFxrSharp.Interop.Methods;

internal static unsafe partial class HostFxrMethods
{
    internal const string LibraryName = "hostfxr";

    [LibraryImport(LibraryName, EntryPoint = "hostfxr_initialize_for_dotnet_command_line")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int HostFxrInitializeForDotnetCommandLine(int argc, byte** argv, HostFxrInitializeParametersNative* parameters, nint* hostContextHandle);

    [LibraryImport(LibraryName, EntryPoint = "hostfxr_initialize_for_runtime_config")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int HostFxrInitializeForRuntimeConfig(byte* runtimeConfigPath, HostFxrInitializeParametersNative* parameters, nint* hostContextHandle);

    [LibraryImport(LibraryName, EntryPoint = "hostfxr_get_runtime_property_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int HostFxrGetRuntimePropertyValue(nint hostContextHandle, byte* name, byte** value);

    [LibraryImport(LibraryName, EntryPoint = "hostfxr_set_runtime_property_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int HostFxrSetRuntimePropertyValue(nint hostContextHandle, byte* name, byte* value);

    [LibraryImport(LibraryName, EntryPoint = "hostfxr_get_runtime_properties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int HostFxrGetRuntimeProperties(nint hostContextHandle, nuint* count, byte** keys, byte** values);

    [LibraryImport(LibraryName, EntryPoint = "hostfxr_run_app")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int HostFxrRunApp(nint hostContextHandle);

    [LibraryImport(LibraryName, EntryPoint = "hostfxr_get_runtime_delegate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int HostFxrGetRuntimeDelegate(nint hostContextHandle, int type, void** result);

    [LibraryImport(LibraryName, EntryPoint = "hostfxr_close")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int HostFxrClose(nint hostContextHandle);

    [LibraryImport(LibraryName, EntryPoint = "hostfxr_set_error_writer")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint HostFxrSetErrorWriter(nint errorWriter);
}
