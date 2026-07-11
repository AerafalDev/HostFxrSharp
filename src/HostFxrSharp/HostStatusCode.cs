using HostFxrSharp.Exceptions;

namespace HostFxrSharp;

/// <summary>
/// The status codes returned by the native hosting components (<c>nethost</c> / <c>hostfxr</c> /
/// <c>hostpolicy</c>).
/// </summary>
/// <remarks>
/// <para>
/// Values mirror <c>enum StatusCode</c> in
/// <see href="https://github.com/dotnet/runtime/blob/main/src/native/corehost/error_codes.h">
/// <c>error_codes.h</c></see>, read from the primary source during Phase&#160;0 research (see
/// <c>docs/RESEARCH.md</c>). The high-bit values (<c>0x8xxxxxxx</c>) are failures; <c>0</c>,
/// <c>1</c> and <c>2</c> are successes, where <see cref="SuccessDifferentRuntimeProperties"/> is a
/// non-fatal warning.
/// </para>
/// <para>
/// The happy-path public API never surfaces a raw status code; this enum is exposed only for
/// diagnostics (for example via <see cref="HostingException.StatusCode"/>) so callers can make
/// structured decisions without dealing with raw integers.
/// </para>
/// </remarks>
public enum HostStatusCode
{
    /// <summary>Operation succeeded (<c>Success</c>, <c>0</c>).</summary>
    Success = 0,

    /// <summary>
    /// The host context attached to an already-initialized runtime (<c>Success_HostAlreadyInitialized</c>,
    /// <c>1</c>). Indicates a <em>secondary</em> host context.
    /// </summary>
    SuccessHostAlreadyInitialized = 1,

    /// <summary>
    /// A secondary host context was created but its runtime configuration specifies properties that
    /// differ from (or are additional to) those already set on the running runtime
    /// (<c>Success_DifferentRuntimeProperties</c>, <c>2</c>). This is a <em>warning</em>: the context
    /// is usable, but the differing properties are ignored by the already-running runtime.
    /// </summary>
    SuccessDifferentRuntimeProperties = 2,

    /// <summary><c>InvalidArgFailure</c> (<c>0x80008081</c>).</summary>
    InvalidArgFailure = unchecked((int)0x80008081),

    /// <summary><c>CoreHostLibLoadFailure</c> (<c>0x80008082</c>).</summary>
    CoreHostLibLoadFailure = unchecked((int)0x80008082),

    /// <summary><c>CoreHostLibMissingFailure</c> (<c>0x80008083</c>): a hosting library could not be found.</summary>
    CoreHostLibMissingFailure = unchecked((int)0x80008083),

    /// <summary><c>CoreHostEntryPointFailure</c> (<c>0x80008084</c>).</summary>
    CoreHostEntryPointFailure = unchecked((int)0x80008084),

    /// <summary><c>CurrentHostFindFailure</c> (<c>0x80008085</c>).</summary>
    CurrentHostFindFailure = unchecked((int)0x80008085),

    /// <summary><c>CoreClrResolveFailure</c> (<c>0x80008087</c>).</summary>
    CoreClrResolveFailure = unchecked((int)0x80008087),

    /// <summary><c>CoreClrBindFailure</c> (<c>0x80008088</c>).</summary>
    CoreClrBindFailure = unchecked((int)0x80008088),

    /// <summary><c>CoreClrInitFailure</c> (<c>0x80008089</c>).</summary>
    CoreClrInitFailure = unchecked((int)0x80008089),

    /// <summary><c>CoreClrExeFailure</c> (<c>0x8000808a</c>).</summary>
    CoreClrExeFailure = unchecked((int)0x8000808a),

    /// <summary><c>ResolverInitFailure</c> (<c>0x8000808b</c>).</summary>
    ResolverInitFailure = unchecked((int)0x8000808b),

    /// <summary><c>ResolverResolveFailure</c> (<c>0x8000808c</c>).</summary>
    ResolverResolveFailure = unchecked((int)0x8000808c),

    /// <summary><c>LibHostInitFailure</c> (<c>0x8000808e</c>).</summary>
    LibHostInitFailure = unchecked((int)0x8000808e),

    /// <summary><c>LibHostInvalidArgs</c> (<c>0x80008092</c>).</summary>
    LibHostInvalidArgs = unchecked((int)0x80008092),

    /// <summary><c>InvalidConfigFile</c> (<c>0x80008093</c>): the <c>.runtimeconfig.json</c> is invalid.</summary>
    InvalidConfigFile = unchecked((int)0x80008093),

    /// <summary><c>AppArgNotRunnable</c> (<c>0x80008094</c>).</summary>
    AppArgNotRunnable = unchecked((int)0x80008094),

    /// <summary><c>AppHostExeNotBoundFailure</c> (<c>0x80008095</c>).</summary>
    AppHostExeNotBoundFailure = unchecked((int)0x80008095),

    /// <summary><c>FrameworkMissingFailure</c> (<c>0x80008096</c>): a required shared framework is missing.</summary>
    FrameworkMissingFailure = unchecked((int)0x80008096),

    /// <summary><c>HostApiFailed</c> (<c>0x80008097</c>).</summary>
    HostApiFailed = unchecked((int)0x80008097),

    /// <summary>
    /// <c>HostApiBufferTooSmall</c> (<c>0x80008098</c>): the supplied buffer was too small; the required
    /// size is returned so the caller can retry. Handled transparently by this library's buffer helpers.
    /// </summary>
    HostApiBufferTooSmall = unchecked((int)0x80008098),

    /// <summary><c>AppPathFindFailure</c> (<c>0x8000809a</c>).</summary>
    AppPathFindFailure = unchecked((int)0x8000809a),

    /// <summary><c>SdkResolveFailure</c> (<c>0x8000809b</c>).</summary>
    SdkResolveFailure = unchecked((int)0x8000809b),

    /// <summary><c>FrameworkCompatFailure</c> (<c>0x8000809c</c>).</summary>
    FrameworkCompatFailure = unchecked((int)0x8000809c),

    /// <summary><c>FrameworkCompatRetry</c> (<c>0x8000809d</c>).</summary>
    FrameworkCompatRetry = unchecked((int)0x8000809d),

    /// <summary><c>BundleExtractionFailure</c> (<c>0x8000809f</c>).</summary>
    BundleExtractionFailure = unchecked((int)0x8000809f),

    /// <summary><c>BundleExtractionIOError</c> (<c>0x800080a0</c>).</summary>
    BundleExtractionIoError = unchecked((int)0x800080a0),

    /// <summary><c>LibHostDuplicateProperty</c> (<c>0x800080a1</c>).</summary>
    LibHostDuplicateProperty = unchecked((int)0x800080a1),

    /// <summary>
    /// <c>HostApiUnsupportedVersion</c> (<c>0x800080a2</c>): a new <c>hostfxr</c> API was invoked against an
    /// older <c>hostpolicy</c> that does not implement it. Mapped to <see cref="IncompatibleHostPolicyException"/>.
    /// </summary>
    HostApiUnsupportedVersion = unchecked((int)0x800080a2),

    /// <summary><c>HostInvalidState</c> (<c>0x800080a3</c>): the operation is invalid in the current host state.</summary>
    HostInvalidState = unchecked((int)0x800080a3),

    /// <summary>
    /// <c>HostPropertyNotFound</c> (<c>0x800080a4</c>): a requested runtime property does not exist.
    /// Mapped to <see cref="RuntimePropertyNotFoundException"/>.
    /// </summary>
    HostPropertyNotFound = unchecked((int)0x800080a4),

    /// <summary><c>HostIncompatibleConfig</c> (<c>0x800080a5</c>).</summary>
    HostIncompatibleConfig = unchecked((int)0x800080a5),

    /// <summary>
    /// <c>HostApiUnsupportedScenario</c> (<c>0x800080a6</c>): the requested scenario is not supported for this
    /// host context (for example an incompatible runtime delegate). Mapped to <see cref="UnsupportedHostingScenarioException"/>.
    /// </summary>
    HostApiUnsupportedScenario = unchecked((int)0x800080a6),

    /// <summary><c>HostFeatureDisabled</c> (<c>0x800080a7</c>).</summary>
    HostFeatureDisabled = unchecked((int)0x800080a7)
}

/// <summary>Convenience classification helpers for <see cref="HostStatusCode"/>.</summary>
public static class HostingStatusCodeExtensions
{
    /// <param name="code">The status code.</param>
    extension(HostStatusCode code)
    {
        /// <summary>Gets a value indicating whether the code represents success (including warnings).</summary>
        /// <returns><see langword="true"/> when the code is a success or warning code; otherwise <see langword="false"/>.</returns>
        public bool IsSuccess()
        {
            return (int)code >= 0;
        }

        /// <summary>Gets a value indicating whether the code represents a failure.</summary>
        /// <returns><see langword="true"/> when the code is a failure code; otherwise <see langword="false"/>.</returns>
        public bool IsFailure()
        {
            return (int)code < 0;
        }

        /// <summary>
        /// Gets a value indicating whether the code indicates a <em>secondary</em> host context
        /// (<see cref="HostStatusCode.SuccessHostAlreadyInitialized"/> or
        /// <see cref="HostStatusCode.SuccessDifferentRuntimeProperties"/>).
        /// </summary>
        /// <returns><see langword="true"/> for a secondary-context code; otherwise <see langword="false"/>.</returns>
        public bool IsSecondaryContext()
        {
            return code is HostStatusCode.SuccessHostAlreadyInitialized or HostStatusCode.SuccessDifferentRuntimeProperties;
        }
    }
}
