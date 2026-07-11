using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HostFxrSharp.Contexts;
using HostFxrSharp.Exceptions;
using HostFxrSharp.Interop;
using HostFxrSharp.Interop.Methods;
using HostFxrSharp.Resolution;

namespace HostFxrSharp;

/// <summary>Receives diagnostic messages emitted by the hosting components (see <see cref="HostFxr.SetErrorWriter"/>).</summary>
/// <param name="message">The error/diagnostic message.</param>
public delegate void HostErrorWriter(string message);

/// <summary>
/// The primary entry point for initializing host contexts and starting the .NET runtime through the native
/// hosting components.
/// </summary>
/// <remarks>
/// <para><b>Single runtime per process.</b> Only one runtime can be loaded per process; the first initialized
/// context owns it (see <see cref="HostContextKind"/>). <c>hostfxr</c> itself is located (via <see cref="NetHost"/>)
/// and loaded once per process by <see cref="EnsureLoaded"/>.</para>
/// <para><b>AOT.</b> All native access uses source-generated P/Invokes and unmanaged function pointers; the only
/// managed→native callback (the error writer) uses a static <see cref="UnmanagedCallersOnlyAttribute"/> method,
/// so the whole surface is Native-AOT compatible.</para>
/// </remarks>
public static class HostFxr
{
    private static readonly Lock LoadLock = new();

    private static string? _loadedHostFxrPath;
    private static HostErrorWriter? _errorWriter;
    private static int _errorWriterInstalled;

    /// <summary>Gets the path of the loaded <c>hostfxr</c> library, or <see langword="null"/> if not yet loaded.</summary>
    public static string? LoadedHostFxrPath =>
        Volatile.Read(ref _loadedHostFxrPath);

    /// <summary>Locates and loads the <c>hostfxr</c> library if it has not been loaded yet (idempotent, process-wide).</summary>
    /// <param name="options">Optional resolution options used only on the first (loading) call.</param>
    /// <exception cref="HostFxrNotFoundException"><c>hostfxr</c> could not be located.</exception>
    /// <exception cref="HostFxrResolutionException">Resolution or loading failed.</exception>
    /// <remarks>Because a process loads a single <c>hostfxr</c>, only the first call's <paramref name="options"/> take effect.</remarks>
    public static void EnsureLoaded(HostFxrResolutionOptions? options = null)
    {
        if (Volatile.Read(ref _loadedHostFxrPath) is not null)
            return;

        lock (LoadLock)
        {
            if (_loadedHostFxrPath is not null)
                return;

            var path = NetHost.GetHostFxrPath(options);
            nint handle;

            try
            {
                handle = NativeLibrary.Load(path);
            }
            catch (Exception ex) when (ex is DllNotFoundException or BadImageFormatException)
            {
                throw new HostFxrResolutionException($"Failed to load hostfxr from '{path}'.", ex);
            }

            NativeLibraryResolver.SetHostFxrHandle(handle);
            Volatile.Write(ref _loadedHostFxrPath, path);
        }
    }

    /// <summary>
    /// Loads <c>hostfxr</c> from an explicit path, bypassing <c>nethost</c> discovery entirely. Use when the
    /// caller already knows where <c>hostfxr</c> lives — an app-local self-contained copy, or one found in the
    /// machine .NET install — so no <c>nethost.dll</c> need be present.
    /// </summary>
    /// <param name="hostFxrPath">Full path to the <c>hostfxr</c> library.</param>
    /// <exception cref="ArgumentException"><paramref name="hostFxrPath"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="HostFxrResolutionException">The library could not be loaded.</exception>
    /// <remarks>Idempotent and process-wide: because a process loads a single <c>hostfxr</c>, only the first call takes effect.</remarks>
    public static void LoadFrom(string hostFxrPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(hostFxrPath);

        if (Volatile.Read(ref _loadedHostFxrPath) is not null)
            return;

        lock (LoadLock)
        {
            if (_loadedHostFxrPath is not null)
                return;

            nint handle;

            try
            {
                handle = NativeLibrary.Load(hostFxrPath);
            }
            catch (Exception ex) when (ex is DllNotFoundException or BadImageFormatException)
            {
                throw new HostFxrResolutionException($"Failed to load hostfxr from '{hostFxrPath}'.", ex);
            }

            NativeLibraryResolver.EnsureInstalled();
            NativeLibraryResolver.SetHostFxrHandle(handle);
            Volatile.Write(ref _loadedHostFxrPath, hostFxrPath);
        }
    }

    /// <summary>Initializes a host context for running a managed application (<c>hostfxr_initialize_for_dotnet_command_line</c>).</summary>
    /// <param name="args">The command-line arguments — for example <c>["app.dll", "arg1", "arg2"]</c>. SDK commands (e.g. <c>dotnet run</c>) are not supported.</param>
    /// <param name="options">Optional initialization options.</param>
    /// <returns>The initialized (first) host context. Use <see cref="HostContext.RunApp"/> to run it.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="args"/> is <see langword="null"/>.</exception>
    /// <exception cref="RuntimeAlreadyLoadedException">A runtime is already loaded in this process.</exception>
    /// <exception cref="HostingException">Initialization failed.</exception>
    /// <remarks>Can only succeed once per process; supports framework-dependent and self-contained apps.</remarks>
    public static HostContext InitializeForCommandLine(string[] args, HostContextOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(args);
        EnsureLoaded(options?.Resolution);
        return HostContext.InitializeForCommandLine(args, options?.HostPath, options?.DotNetRoot);
    }

    /// <summary>Initializes a host context for a component/runtime configuration (<c>hostfxr_initialize_for_runtime_config</c>).</summary>
    /// <param name="runtimeConfigPath">Path to the <c>.runtimeconfig.json</c>. Must describe a framework-dependent component (self-contained is rejected).</param>
    /// <param name="options">Optional initialization options.</param>
    /// <returns>
    /// The initialized host context. Inspect <see cref="HostContext.Kind"/> for first vs. secondary and
    /// <see cref="HostContext.RuntimePropertiesDiffer"/> for the divergent-properties warning.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="runtimeConfigPath"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="HostingException">Initialization failed.</exception>
    /// <remarks>May be called multiple times per process and is safe to call concurrently from multiple threads.</remarks>
    public static HostContext InitializeForRuntimeConfig(string runtimeConfigPath, HostContextOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(runtimeConfigPath);
        EnsureLoaded(options?.Resolution);
        return HostContext.InitializeForRuntimeConfig(runtimeConfigPath, options?.HostPath, options?.DotNetRoot);
    }

    /// <summary>Attempts to read a runtime property from the process's first (active) host context.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The property value if found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the property exists; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/> or empty.</exception>
    /// <remarks>Operates on the active context (native <c>host_context_handle == NULL</c>); requires a loaded runtime.</remarks>
    public static bool TryGetActiveRuntimeProperty(string name, [NotNullWhen(true)] out string? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var rc = NativeHostingApi.GetRuntimePropertyValue(nint.Zero, name, out value);
        var code = (HostStatusCode)rc;

        if (code.IsSuccess())
            return value is not null;

        if (code is HostStatusCode.HostPropertyNotFound)
        {
            value = null;
            return false;
        }

        throw HostingErrorMapper.ToException(code, "hostfxr_get_runtime_property_value");
    }

    /// <summary>Reads all runtime properties from the process's first (active) host context.</summary>
    /// <returns>A snapshot of the active runtime properties.</returns>
    /// <remarks>Operates on the active context (native <c>host_context_handle == NULL</c>); requires a loaded runtime.</remarks>
    public static IReadOnlyDictionary<string, string> GetActiveRuntimeProperties()
    {
        return HostContext.ReadActiveRuntimeProperties();
    }

    /// <summary>Installs a process-wide diagnostic error writer for the hosting components.</summary>
    /// <param name="writer">The callback that receives host diagnostic messages.</param>
    /// <returns>A registration that, when disposed, removes the writer.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Demonstrates the AOT-safe managed→native callback pattern: a static
    /// <see cref="UnmanagedCallersOnlyAttribute"/> trampoline routes to the supplied <see cref="HostErrorWriter"/>.
    /// The native error writer is process-global, so the most recent registration wins.
    /// </remarks>
    public static IDisposable SetErrorWriter(HostErrorWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        EnsureLoaded();
        Volatile.Write(ref _errorWriter, writer);

        if (Interlocked.Exchange(ref _errorWriterInstalled, 1) is 0)
            InstallErrorWriterTrampoline();

        return new ErrorWriterRegistration();
    }

    private static unsafe void InstallErrorWriterTrampoline()
    {
        _ = HostFxrMethods.HostFxrSetErrorWriter((nint)(delegate* unmanaged[Cdecl]<byte*, void>)&ErrorWriterTrampoline);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void ErrorWriterTrampoline(byte* message)
    {
        var writer = Volatile.Read(ref _errorWriter);

        if (writer is null)
            return;

        var text = NativeString.Read(message) ?? string.Empty;

        try
        {
            writer(text);
        }
        catch (Exception)
        {
            // Intentionally swallowed: crossing the native->managed boundary with an exception is undefined.
        }
    }

    private sealed class ErrorWriterRegistration : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) is 0)
                Volatile.Write(ref _errorWriter, null);
        }
    }
}
