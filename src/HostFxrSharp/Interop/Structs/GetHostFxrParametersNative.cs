using System.Runtime.InteropServices;

namespace HostFxrSharp.Interop.Structs;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct GetHostFxrParametersNative
{
    public nuint Size;

    public byte* AssemblyPath;

    public byte* DotNetRoot;
}
