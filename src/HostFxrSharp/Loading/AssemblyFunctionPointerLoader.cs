using HostFxrSharp.Exceptions;
using HostFxrSharp.Interop;

namespace HostFxrSharp.Loading;

/// <summary>
/// Typed wrapper around the <c>load_assembly_and_get_function_pointer_fn</c> runtime helper
/// (delegate type <see cref="HostFxrDelegateType.LoadAssemblyAndGetFunctionPointer"/>).
/// </summary>
/// <remarks>
/// <para>
/// Wraps a native <c>delegate* unmanaged[Stdcall]&lt;byte*, byte*, byte*, byte*, void*, void**, int&gt;</c>
/// function pointer — <b>no managed delegate marshalling</b>, so it is fully Native-AOT compatible.
/// The pointer has process lifetime; the loaded managed component cannot be unloaded.
/// </para>
/// <para>The target assembly is always loaded into an <b>isolated</b> <c>AssemblyLoadContext</c> with
/// <c>AssemblyDependencyResolver</c> applied from its <c>.deps.json</c>.</para>
/// </remarks>
public readonly unsafe struct AssemblyFunctionPointerLoader : IEquatable<AssemblyFunctionPointerLoader>
{
    private readonly delegate* unmanaged[Stdcall]<byte*, byte*, byte*, byte*, void*, void**, int> _fn;

    /// <summary>Gets a value indicating whether this wrapper holds a valid function pointer.</summary>
    public bool IsValid =>
        _fn is not null;

    /// <summary>Gets the underlying native function pointer.</summary>
    public nint UnderlyingPointer =>
        (nint)_fn;

    internal AssemblyFunctionPointerLoader(nint functionPointer)
    {
        _fn = (delegate* unmanaged[Stdcall]<byte*, byte*, byte*, byte*, void*, void**, int>)functionPointer;
    }

    /// <summary>Loads the assembly in isolation and returns a native function pointer to the specified static method.</summary>
    /// <param name="assemblyPath">Path to the component's main assembly (the one next to its <c>.deps.json</c>).</param>
    /// <param name="typeName">Assembly-qualified type name that declares the method.</param>
    /// <param name="methodName">Name of the <see langword="static"/> method to locate.</param>
    /// <param name="signature">Describes the method signature. Defaults to <see cref="MethodSignature.Default"/>.</param>
    /// <returns>A native function pointer to the managed method.</returns>
    /// <exception cref="ArgumentException">A required string argument is <see langword="null"/> or empty.</exception>
    /// <exception cref="InvalidOperationException">This wrapper was not obtained from a host context.</exception>
    /// <exception cref="HostingException">The native call failed.</exception>
    public nint Load(string assemblyPath, string typeName, string methodName, MethodSignature signature = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(assemblyPath);
        ArgumentException.ThrowIfNullOrEmpty(typeName);
        ArgumentException.ThrowIfNullOrEmpty(methodName);

        if (_fn is null)
            throw new InvalidOperationException("This loader was not obtained from a host context.");

        var ap = NativeString.Allocate(assemblyPath);
        var tn = NativeString.Allocate(typeName);
        var mn = NativeString.Allocate(methodName);
        var dtnBuffer = NativeString.Allocate(signature.DelegateTypeName);

        try
        {
            var dtn = signature.ResolveNativePointer(dtnBuffer);
            var result = (void*)nint.Zero;
            var rc = _fn(ap, tn, mn, dtn, null, &result);
            HostingError.ThrowIfFailed(rc, "load_assembly_and_get_function_pointer");
            return (nint)result;
        }
        finally
        {
            NativeString.Free(ap);
            NativeString.Free(tn);
            NativeString.Free(mn);
            NativeString.Free(dtnBuffer);
        }
    }

    /// <inheritdoc/>
    public bool Equals(AssemblyFunctionPointerLoader other)
    {
        return (nint)_fn == (nint)other._fn;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is AssemblyFunctionPointerLoader other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return ((nint)_fn).GetHashCode();
    }

    /// <summary>Determines whether two wrappers reference the same function pointer.</summary>
    /// <param name="left">The first wrapper.</param>
    /// <param name="right">The second wrapper.</param>
    /// <returns><see langword="true"/> if equal; otherwise <see langword="false"/>.</returns>
    public static bool operator ==(AssemblyFunctionPointerLoader left, AssemblyFunctionPointerLoader right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two wrappers reference different function pointers.</summary>
    /// <param name="left">The first wrapper.</param>
    /// <param name="right">The second wrapper.</param>
    /// <returns><see langword="true"/> if not equal; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(AssemblyFunctionPointerLoader left, AssemblyFunctionPointerLoader right)
    {
        return !left.Equals(right);
    }
}
