using System.Runtime.InteropServices;
using System.Text;

namespace HostFxrSharp.Interop;

internal static unsafe class NativeString
{
    internal static bool IsUtf16 =>
        OperatingSystem.IsWindows();

    internal static int UnitSize =>
        IsUtf16 ? 2 : 1;

    internal static byte* Allocate(string? value)
    {
        if (value is null)
            return null;

        return IsUtf16
            ? (byte*)Marshal.StringToCoTaskMemUni(value)
            : (byte*)Marshal.StringToCoTaskMemUTF8(value);
    }

    internal static void Free(byte* buffer)
    {
        if (buffer is not null)
            Marshal.FreeCoTaskMem((nint)buffer);
    }

    internal static string? Read(byte* buffer)
    {
        if (buffer is null)
            return null;

        return IsUtf16
            ? Marshal.PtrToStringUni((nint)buffer)
            : Marshal.PtrToStringUTF8((nint)buffer);
    }

    internal static string Decode(byte* buffer, int units)
    {
        if (buffer is null || units <= 0)
            return string.Empty;

        return IsUtf16
            ? new string((char*)buffer, 0, units)
            : Encoding.UTF8.GetString(buffer, units);
    }

    internal static string? RoundTrip(string? value)
    {
        var buffer = Allocate(value);

        try
        {
            return Read(buffer);
        }
        finally
        {
            Free(buffer);
        }
    }
}
