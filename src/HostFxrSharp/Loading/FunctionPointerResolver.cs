using HostFxrSharp.Exceptions;
using HostFxrSharp.Interop;

namespace HostFxrSharp.Loading;

/// <summary>
/// Typed wrapper around the <c>get_function_pointer_fn</c> runtime helper (delegate type
/// <see cref="HostFxrDelegateType.GetFunctionPointer"/>, .NET 5+).
/// </summary>
/// <remarks>
/// Resolves a type/method from the <b>default</b> <c>AssemblyLoadContext</c> and returns a native function
/// pointer. Wraps <c>delegate* unmanaged[Stdcall]&lt;byte*, byte*, byte*, void*, void*, void**, int&gt;</c>
/// (native <c>char_t*</c> arguments are marshalled per platform).
/// </remarks>
public readonly unsafe struct FunctionPointerResolver : IEquatable<FunctionPointerResolver>
{
    private readonly delegate* unmanaged[Stdcall]<byte*, byte*, byte*, void*, void*, void**, int> _fn;

    /// <summary>Gets a value indicating whether this wrapper holds a valid function pointer.</summary>
    public bool IsValid =>
        _fn is not null;

    /// <summary>Gets the underlying native function pointer.</summary>
    public nint UnderlyingPointer =>
        (nint)_fn;

    internal FunctionPointerResolver(nint functionPointer)
    {
        _fn = (delegate* unmanaged[Stdcall]<byte*, byte*, byte*, void*, void*, void**, int>)functionPointer;
    }

    /// <summary>Resolves a static method in the default load context and returns a native function pointer to it.</summary>
    /// <param name="typeName">Assembly-qualified type name that declares the method.</param>
    /// <param name="methodName">Name of the <see langword="static"/> method to locate.</param>
    /// <param name="signature">Describes the method signature. Defaults to <see cref="MethodSignature.Default"/>.</param>
    /// <returns>A native function pointer to the managed method.</returns>
    /// <exception cref="ArgumentException">A required string argument is <see langword="null"/> or empty.</exception>
    /// <exception cref="InvalidOperationException">This wrapper was not obtained from a host context.</exception>
    /// <exception cref="HostingException">The native call failed.</exception>
    public nint Resolve(string typeName, string methodName, MethodSignature signature = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(typeName);
        ArgumentException.ThrowIfNullOrEmpty(methodName);

        if (_fn is null)
            throw new InvalidOperationException("This resolver was not obtained from a host context.");

        var tn = NativeString.Allocate(typeName);
        var mn = NativeString.Allocate(methodName);
        var dtnBuffer = NativeString.Allocate(signature.DelegateTypeName);

        try
        {
            var dtn = signature.ResolveNativePointer(dtnBuffer);
            var result = (void*)nint.Zero;
            var rc = _fn(tn, mn, dtn, null, null, &result);
            HostingError.ThrowIfFailed(rc, "get_function_pointer");
            return (nint)result;
        }
        finally
        {
            NativeString.Free(tn);
            NativeString.Free(mn);
            NativeString.Free(dtnBuffer);
        }
    }

    /// <inheritdoc/>
    public bool Equals(FunctionPointerResolver other)
    {
        return (nint)_fn == (nint)other._fn;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is FunctionPointerResolver other && Equals(other);
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
    public static bool operator ==(FunctionPointerResolver left, FunctionPointerResolver right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two wrappers reference different function pointers.</summary>
    /// <param name="left">The first wrapper.</param>
    /// <param name="right">The second wrapper.</param>
    /// <returns><see langword="true"/> if not equal; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(FunctionPointerResolver left, FunctionPointerResolver right)
    {
        return !left.Equals(right);
    }
}
