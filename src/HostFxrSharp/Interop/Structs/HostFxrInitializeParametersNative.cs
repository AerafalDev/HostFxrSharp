using System.Runtime.InteropServices;

namespace HostFxrSharp.Interop.Structs;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct HostFxrInitializeParametersNative
{
    public nuint Size;

    public byte* HostPath;

    public byte* DotNetRoot;
}
