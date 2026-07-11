using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using HostFxrSharp.Exceptions;
using HostFxrSharp.Interop;
using HostFxrSharp.Loading;

namespace HostFxrSharp.Contexts;

/// <summary>
/// An initialized <c>hostfxr</c> host context: the safe, disposable owner of a native
/// <c>hostfxr_handle</c> through which the runtime is started, runtime properties are inspected/modified,
/// and runtime delegates are obtained.
/// </summary>
/// <remarks>
/// <para><b>Threading.</b> A host context (like the native handle it wraps) is <b>not thread-safe</b>: call at
/// most one method at a time from a single thread. Different contexts may be used from different threads.</para>
/// <para><b>Lifetime.</b> Always <see cref="Dispose"/> a context. The underlying handle is a
/// <see cref="SafeHandle"/>, so <c>hostfxr_close</c> runs exactly once even if disposal is missed; however an
/// <em>abandoned</em> first context can block future initializations at the native layer, so deterministic
/// disposal is important. Native property-value pointers are copied to managed strings before return, honoring
/// the native "valid only until the next run/close/set" rule.</para>
/// <para><b>Runtime start.</b> Requesting any runtime delegate (or calling <see cref="RunApp"/>) starts the
/// runtime; after that, runtime properties become read-only.</para>
/// </remarks>
public sealed class HostContext : IDisposable
{
    private static readonly IReadOnlyDictionary<string, string> EmptyProperties = ReadOnlyDictionary<string, string>.Empty;

    private readonly HostContextHandle _handle;
    private readonly HostContextInitKind _initKind;

    private int _runInvoked;
    private int _runtimeStarted;
    private int _disposed;

    internal bool CanModifyRuntimeProperties =>
        !IsDisposed && Kind is HostContextKind.First && Volatile.Read(ref _runtimeStarted) is 0;

    /// <summary>Gets whether this is the process's first (runtime-owning) context or a secondary one.</summary>
    public HostContextKind Kind { get; }

    /// <summary>
    /// Gets a value indicating whether this secondary context's runtime configuration specifies properties that
    /// differ from those already set on the running runtime (native <see cref="HostStatusCode.SuccessDifferentRuntimeProperties"/>).
    /// The differing properties are ignored by the running runtime; use <see cref="GetConflictingRuntimeProperties"/>
    /// to enumerate them and decide whether to proceed.
    /// </summary>
    public bool RuntimePropertiesDiffer { get; }

    /// <summary>Gets the status code returned by the initialization call that created this context.</summary>
    public HostStatusCode InitializationStatus { get; }

    /// <summary>Gets the runtime property accessor for this context.</summary>
    public RuntimeProperties Properties { get; }

    /// <summary>Gets a value indicating whether this context has been disposed.</summary>
    public bool IsDisposed =>
        Volatile.Read(ref _disposed) is not 0;

    private HostContext(nint handle, HostStatusCode initStatus, HostContextInitKind initKind)
    {
        _handle = new HostContextHandle(handle);
        _initKind = initKind;
        InitializationStatus = initStatus;
        Kind = initStatus.IsSecondaryContext() ? HostContextKind.Secondary : HostContextKind.First;
        RuntimePropertiesDiffer = initStatus is HostStatusCode.SuccessDifferentRuntimeProperties;
        Properties = new RuntimeProperties(this);
    }

    internal static HostContext InitializeForRuntimeConfig(string runtimeConfigPath, string? hostPath, string? dotnetRoot)
    {
        var rc = NativeHostingApi.InitializeForRuntimeConfig(runtimeConfigPath, hostPath, dotnetRoot, out var handle);
        var code = (HostStatusCode)rc;

        if (code.IsFailure())
            throw HostingErrorMapper.ToException(code, "hostfxr_initialize_for_runtime_config", HostContextInitKind.RuntimeConfig);

        return new HostContext(handle, code, HostContextInitKind.RuntimeConfig);
    }

    internal static HostContext InitializeForCommandLine(string[] args, string? hostPath, string? dotnetRoot)
    {
        var rc = NativeHostingApi.InitializeForCommandLine(args, hostPath, dotnetRoot, out var handle);
        var code = (HostStatusCode)rc;

        if (code.IsFailure())
            throw HostingErrorMapper.ToException(code, "hostfxr_initialize_for_dotnet_command_line", HostContextInitKind.CommandLine);

        return new HostContext(handle, code, HostContextInitKind.CommandLine);
    }

    /// <summary>
    /// Runs the application specified when the context was created via
    /// <see cref="HostFxr.InitializeForCommandLine(string[], HostContextOptions?)"/>. Blocks until the app exits.
    /// </summary>
    /// <returns>The application's exit code. Host-layer startup failures are reflected as the corresponding native status value.</returns>
    /// <exception cref="ObjectDisposedException">The context has been disposed.</exception>
    /// <exception cref="InvalidOperationException">
    /// The context was not created for command-line execution, or a run operation was already invoked.
    /// </exception>
    /// <remarks>Can be called at most once and cannot be combined with any other run operation.</remarks>
    public int RunApp()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (_initKind is not HostContextInitKind.CommandLine)
            throw new InvalidOperationException("RunApp can only be used on a host context created via HostFxr.InitializeForCommandLine.");

        if (Interlocked.Exchange(ref _runInvoked, 1) is not 0)
            throw new InvalidOperationException("RunApp can only be called once per host context and cannot be combined with other run operations.");

        Volatile.Write(ref _runtimeStarted, 1);
        using var lease = Lease();
        return NativeHostingApi.RunApp(lease.Handle);
    }

    /// <summary>Starts the runtime (if not already) and returns a raw native function pointer for the requested delegate type.</summary>
    /// <param name="type">The runtime delegate type to request.</param>
    /// <returns>The raw native function pointer. Cast it to the documented native signature for the delegate type.</returns>
    /// <exception cref="ObjectDisposedException">The context has been disposed.</exception>
    /// <exception cref="UnsupportedHostingScenarioException">The delegate type is unavailable on the current runtime (for example WinRT activation on .NET 5+).</exception>
    /// <exception cref="HostingException">The native call failed.</exception>
    /// <remarks>Prefer the typed helpers (<see cref="GetAssemblyFunctionPointerLoader"/>, etc.) for the four data-loading delegates.</remarks>
    public nint GetRuntimeDelegate(HostFxrDelegateType type)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (type is HostFxrDelegateType.WinRtActivation && Environment.Version.Major >= 5)
            throw new UnsupportedHostingScenarioException("The WinRT activation delegate (hdt_winrt_activation) is only available on .NET Core 3.x and was removed in .NET 5 and later.");

        Volatile.Write(ref _runtimeStarted, 1);
        using var lease = Lease();
        var rc = NativeHostingApi.GetRuntimeDelegate(lease.Handle, type, out var result);
        HostingError.ThrowIfFailed(rc, "hostfxr_get_runtime_delegate");
        return result;
    }

    /// <summary>Gets a typed wrapper for <c>load_assembly_and_get_function_pointer</c> (isolated ALC load + static method pointer).</summary>
    /// <returns>An <see cref="AssemblyFunctionPointerLoader"/>.</returns>
    public AssemblyFunctionPointerLoader GetAssemblyFunctionPointerLoader()
    {
        return new AssemblyFunctionPointerLoader(GetRuntimeDelegate(HostFxrDelegateType.LoadAssemblyAndGetFunctionPointer));
    }

    /// <summary>Gets a typed wrapper for <c>get_function_pointer</c> (default-ALC method pointer, .NET 5+).</summary>
    /// <returns>A <see cref="FunctionPointerResolver"/>.</returns>
    public FunctionPointerResolver GetFunctionPointerResolver()
    {
        return new FunctionPointerResolver(GetRuntimeDelegate(HostFxrDelegateType.GetFunctionPointer));
    }

    /// <summary>Gets a typed wrapper for <c>load_assembly</c> (default-ALC load by path, .NET 8+).</summary>
    /// <returns>An <see cref="AssemblyLoader"/>.</returns>
    public AssemblyLoader GetAssemblyLoader()
    {
        return new AssemblyLoader(GetRuntimeDelegate(HostFxrDelegateType.LoadAssembly));
    }

    /// <summary>Gets a typed wrapper for <c>load_assembly_bytes</c> (default-ALC load from memory, .NET 8+).</summary>
    /// <returns>An <see cref="AssemblyBytesLoader"/>.</returns>
    public AssemblyBytesLoader GetAssemblyBytesLoader()
    {
        return new AssemblyBytesLoader(GetRuntimeDelegate(HostFxrDelegateType.LoadAssemblyBytes));
    }

    /// <summary>
    /// For a secondary context with <see cref="RuntimePropertiesDiffer"/>, returns the runtime properties whose
    /// requested value differs from (or is missing on) the running runtime, so the host can decide whether to proceed.
    /// </summary>
    /// <returns>The list of conflicts (empty for a first context or when properties do not differ).</returns>
    /// <exception cref="ObjectDisposedException">The context has been disposed.</exception>
    public IReadOnlyList<RuntimePropertyConflict> GetConflictingRuntimeProperties()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (Kind is not HostContextKind.Secondary || !RuntimePropertiesDiffer)
            return [];

        var requested = GetAllRuntimeProperties();
        var active = ReadActiveRuntimeProperties();

        var conflicts = new List<RuntimePropertyConflict>();

        foreach (var kv in requested)
        {
            active.TryGetValue(kv.Key, out var activeValue);

            if (activeValue is null || !string.Equals(activeValue, kv.Value, StringComparison.Ordinal))
                conflicts.Add(new RuntimePropertyConflict(kv.Key, kv.Value, activeValue));
        }

        return conflicts;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) is 0)
            _handle.Dispose();
    }

    internal bool TryGetRuntimeProperty(string name, out string? value)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentException.ThrowIfNullOrEmpty(name);

        using var lease = Lease();
        var rc = NativeHostingApi.GetRuntimePropertyValue(lease.Handle, name, out value);
        var code = (HostStatusCode)rc;

        if (code.IsSuccess())
            return true;

        if (code is HostStatusCode.HostPropertyNotFound)
        {
            value = null;
            return false;
        }

        throw HostingErrorMapper.ToException(code, "hostfxr_get_runtime_property_value");
    }

    internal string GetRuntimeProperty(string name)
    {
        if (TryGetRuntimeProperty(name, out var value) && value is not null)
            return value;

        throw RuntimePropertyNotFoundException.ForProperty(name);
    }

    internal void SetRuntimeProperty(string name, string? value)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (Kind is not HostContextKind.First)
            throw new InvalidOperationException("Runtime properties can only be modified on the first host context in the process; secondary contexts are read-only.");

        if (Volatile.Read(ref _runtimeStarted) is not 0)
            throw new InvalidOperationException("Runtime properties cannot be modified after the runtime has started (a runtime delegate was requested or the app was run).");

        using var lease = Lease();
        var rc = NativeHostingApi.SetRuntimePropertyValue(lease.Handle, name, value);
        HostingError.ThrowIfFailed(rc, "hostfxr_set_runtime_property_value");
    }

    internal IReadOnlyDictionary<string, string> GetAllRuntimeProperties()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        using var lease = Lease();
        return ReadProperties(lease.Handle, "hostfxr_get_runtime_properties");
    }

    internal static IReadOnlyDictionary<string, string> ReadActiveRuntimeProperties()
    {
        return ReadProperties(nint.Zero, "hostfxr_get_runtime_properties");
    }

    private static IReadOnlyDictionary<string, string> ReadProperties(nint handle, string operation)
    {
        nuint count = 0;
        var rc = NativeHostingApi.GetRuntimeProperties(handle, ref count, null, null);
        var code = (HostStatusCode)rc;

        if (code.IsFailure() && code is not HostStatusCode.HostApiBufferTooSmall)
            throw HostingErrorMapper.ToException(code, operation);

        if (count is 0)
            return EmptyProperties;

        while (true)
        {
            var capacity = (int)count;
            var keys = new string[capacity];
            var values = new string[capacity];
            var used = count;

            rc = NativeHostingApi.GetRuntimeProperties(handle, ref used, keys, values);
            code = (HostStatusCode)rc;

            if (code is HostStatusCode.HostApiBufferTooSmall)
            {
                count = used;
                continue;
            }

            if (code.IsFailure())
                throw HostingErrorMapper.ToException(code, operation);

            var written = (int)Math.Min(used, (nuint)capacity);
            var dict = new Dictionary<string, string>(written, StringComparer.Ordinal);

            for (var i = 0; i < written; i++)
                dict[keys[i]] = values[i];

            return dict;
        }
    }

    private HandleLease Lease()
    {
        return new HandleLease(_handle);
    }

    private readonly struct HandleLease : IDisposable
    {
        private readonly SafeHandle _safeHandle;
        private readonly bool _refAdded;

        public nint Handle { get; }

        public HandleLease(SafeHandle safeHandle)
        {
            _safeHandle = safeHandle;
            _refAdded = false;
            safeHandle.DangerousAddRef(ref _refAdded);
            Handle = safeHandle.DangerousGetHandle();
        }

        public void Dispose()
        {
            if (_refAdded)
                _safeHandle.DangerousRelease();
        }
    }
}
