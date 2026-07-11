using HostFxrSharp.Exceptions;

namespace HostFxrSharp.Interop;

internal static class HostingErrorMapper
{
    internal static HostingException ToException(HostStatusCode code, string operation, HostContextInitKind? initKind = null)
    {
        var baseMessage = $"Native hosting operation '{operation}' failed with status {code} (0x{(uint)code:X8}).";

        return code switch
        {
            HostStatusCode.HostApiUnsupportedVersion => new IncompatibleHostPolicyException($"{baseMessage} The resolved hostpolicy is older than the hostfxr API being used. The new hostfxr_initialize_* APIs require a matching (newer) hostpolicy; the application likely resolved to an older shared framework.", code),
            HostStatusCode.HostApiUnsupportedScenario => new UnsupportedHostingScenarioException($"{baseMessage} This scenario is not supported for the given host context (for example requesting a runtime delegate that is incompatible with how the context was initialized, or a self-contained component).", code),
            HostStatusCode.HostPropertyNotFound => new RuntimePropertyNotFoundException(baseMessage),
            HostStatusCode.CoreHostLibMissingFailure => new HostFxrNotFoundException($"{baseMessage} A required hosting library (hostfxr/hostpolicy) could not be found."),
            HostStatusCode.HostInvalidState => initKind is HostContextInitKind.CommandLine
                ? new RuntimeAlreadyLoadedException($"{baseMessage} A runtime is already loaded in this process. hostfxr_initialize_for_dotnet_command_line can only initialize the first host context and fails if CoreCLR is already running.", code)
                : new HostInvalidStateException(baseMessage, code),
            _ => new HostingApiException(baseMessage, code)
        };
    }
}
