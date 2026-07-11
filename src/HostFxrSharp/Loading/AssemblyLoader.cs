using HostFxrSharp.Exceptions;
using HostFxrSharp.Interop;

namespace HostFxrSharp.Loading;

/// <summary>
/// Typed wrapper around the <c>load_assembly_fn</c> runtime helper (delegate type
/// <see cref="HostFxrDelegateType.LoadAssembly"/>, .NET 8+).
/// </summary>
/// <remarks>
/// Loads an assembly by path into the <b>default</b> load context, registering
/// <c>AssemblyDependencyResolver</c> from the sibling <c>.deps.json</c>. Wraps
/// <c>delegate* unmanaged[Stdcall]&lt;byte*, void*, void*, int&gt;</c> (the native <c>char_t*</c> path is
/// marshalled per platform).
/// </remarks>
public readonly unsafe struct AssemblyLoader : IEquatable<AssemblyLoader>
{
    private readonly delegate* unmanaged[Stdcall]<byte*, void*, void*, int> _fn;

    /// <summary>Gets a value indicating whether this wrapper holds a valid function pointer.</summary>
    public bool IsValid =>
        _fn is not null;

    /// <summary>Gets the underlying native function pointer.</summary>
    public nint UnderlyingPointer =>
        (nint)_fn;

    internal AssemblyLoader(nint functionPointer)
    {
        _fn = (delegate* unmanaged[Stdcall]<byte*, void*, void*, int>)functionPointer;
    }

    /// <summary>Loads the assembly at the specified path into the default load context.</summary>
    /// <param name="assemblyPath">Path to the assembly to load (matching <c>AssemblyLoadContext.LoadFromAssemblyPath</c> semantics).</param>
    /// <exception cref="ArgumentException"><paramref name="assemblyPath"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="InvalidOperationException">This wrapper was not obtained from a host context.</exception>
    /// <exception cref="HostingException">The native call failed.</exception>
    public void Load(string assemblyPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(assemblyPath);

        if (_fn is null)
            throw new InvalidOperationException("This loader was not obtained from a host context.");

        var ap = NativeString.Allocate(assemblyPath);

        try
        {
            var rc = _fn(ap, null, null);
            HostingError.ThrowIfFailed(rc, "load_assembly");
        }
        finally
        {
            NativeString.Free(ap);
        }
    }

    /// <inheritdoc/>
    public bool Equals(AssemblyLoader other)
    {
        return (nint)_fn == (nint)other._fn;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is AssemblyLoader other && Equals(other);
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
    public static bool operator ==(AssemblyLoader left, AssemblyLoader right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two wrappers reference different function pointers.</summary>
    /// <param name="left">The first wrapper.</param>
    /// <param name="right">The second wrapper.</param>
    /// <returns><see langword="true"/> if not equal; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(AssemblyLoader left, AssemblyLoader right)
    {
        return !left.Equals(right);
    }
}
