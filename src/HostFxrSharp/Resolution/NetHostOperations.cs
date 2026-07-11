using System.Buffers;
using HostFxrSharp.Exceptions;
using HostFxrSharp.Interop;

namespace HostFxrSharp.Resolution;

internal static unsafe class NetHostOperations
{
    private const int StackBufferBytes = 512;
    private const int MaxBufferBytes = 1 << 20;

    internal static string GetHostFxrPath(string? assemblyPath, string? dotnetRoot)
    {
        var unitSize = NativeString.UnitSize;

        Span<byte> stackBuffer = stackalloc byte[StackBufferBytes];
        byte[]? rented = null;

        try
        {
            var buffer = stackBuffer;

            while (true)
            {
                var capacityUnits = (nuint)(buffer.Length / unitSize);
                var used = capacityUnits;
                int rc;

                fixed (byte* buf = buffer)
                    rc = NativeHostingApi.GetHostFxrPath(buf, ref used, assemblyPath, dotnetRoot);

                var code = (HostStatusCode)rc;

                if (code is HostStatusCode.Success)
                {
                    var units = used is 0 ? 0 : (int)Math.Min(used - 1, capacityUnits);

                    fixed (byte* buf = buffer)
                        return NativeString.Decode(buf, units);
                }

                if (code is HostStatusCode.HostApiBufferTooSmall)
                {
                    var requiredUnits = (long)Math.Min(used, (nuint)int.MaxValue);
                    var requiredBytes = requiredUnits * unitSize;

                    if (requiredBytes <= buffer.Length)
                        requiredBytes = buffer.Length * 2L;

                    if (requiredBytes > MaxBufferBytes)
                        throw new HostFxrResolutionException($"The hostfxr path length exceeds the supported maximum of {MaxBufferBytes} bytes.");

                    if (rented is not null)
                        ArrayPool<byte>.Shared.Return(rented);

                    rented = ArrayPool<byte>.Shared.Rent((int)requiredBytes);
                    buffer = rented;
                    continue;
                }

                throw MapFailure(code);
            }
        }
        finally
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static HostingException MapFailure(HostStatusCode code)
    {
        return code switch
        {
            HostStatusCode.CoreHostLibMissingFailure => new HostFxrNotFoundException(),
            HostStatusCode.CurrentHostFindFailure => new HostFxrNotFoundException("The current host could not be located while resolving hostfxr."),
            _ => new HostFxrResolutionException($"nethost get_hostfxr_path failed with status {code} (0x{(uint)code:X8}).", code)
        };
    }
}
