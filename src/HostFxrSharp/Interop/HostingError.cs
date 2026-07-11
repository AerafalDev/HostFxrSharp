namespace HostFxrSharp.Interop;

internal static class HostingError
{
    internal static void ThrowIfFailed(int rawStatus, string operation, HostContextInitKind? initKind = null)
    {
        var code = (HostStatusCode)rawStatus;

        if (code.IsFailure())
            throw HostingErrorMapper.ToException(code, operation, initKind);
    }
}
