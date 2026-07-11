using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HostFxrSharp.Interop.Structs;

namespace HostFxrSharp.Interop.Methods;

internal static unsafe partial class NetHostMethods
{
    internal const string LibraryName = "nethost";

    [LibraryImport(LibraryName, EntryPoint = "get_hostfxr_path")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial int GetHostFxrPath(byte* buffer, nuint* bufferSize, GetHostFxrParametersNative* parameters);
}
