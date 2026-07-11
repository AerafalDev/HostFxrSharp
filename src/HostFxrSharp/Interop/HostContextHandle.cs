using System.Runtime.InteropServices;

namespace HostFxrSharp.Interop;

internal sealed class HostContextHandle : SafeHandle
{
    public override bool IsInvalid =>
        handle == nint.Zero;

    internal HostContextHandle(nint handle) : base(invalidHandleValue: nint.Zero, ownsHandle: true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        return ((HostStatusCode)NativeHostingApi.Close(handle)).IsSuccess();
    }
}
