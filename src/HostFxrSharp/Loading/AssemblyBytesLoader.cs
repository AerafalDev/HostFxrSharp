using HostFxrSharp.Exceptions;
using HostFxrSharp.Interop;

namespace HostFxrSharp.Loading;

/// <summary>
/// Typed wrapper around the <c>load_assembly_bytes_fn</c> runtime helper (delegate type
/// <see cref="HostFxrDelegateType.LoadAssemblyBytes"/>, .NET 8+).
/// </summary>
/// <remarks>
/// Loads an assembly from a byte array into the <b>default</b> load context. Provides <b>no</b> file-based
/// dependency resolution — dependencies must be pre-loaded or resolved by the assembly itself. Wraps
/// <c>delegate* unmanaged[Stdcall]&lt;void*, nuint, void*, nuint, void*, void*, int&gt;</c>.
/// </remarks>
public readonly unsafe struct AssemblyBytesLoader : IEquatable<AssemblyBytesLoader>
{
    private readonly delegate* unmanaged[Stdcall]<void*, nuint, void*, nuint, void*, void*, int> _fn;

    /// <summary>Gets a value indicating whether this wrapper holds a valid function pointer.</summary>
    public bool IsValid =>
        _fn is not null;

    /// <summary>Gets the underlying native function pointer.</summary>
    public nint UnderlyingPointer =>
        (nint)_fn;

    internal AssemblyBytesLoader(nint functionPointer)
    {
        _fn = (delegate* unmanaged[Stdcall]<void*, nuint, void*, nuint, void*, void*, int>)functionPointer;
    }

    /// <summary>Loads an assembly (and optional symbols) from memory into the default load context.</summary>
    /// <param name="assembly">The raw assembly bytes. Must not be empty.</param>
    /// <param name="symbols">Optional raw symbol (PDB) bytes.</param>
    /// <exception cref="ArgumentException"><paramref name="assembly"/> is empty.</exception>
    /// <exception cref="InvalidOperationException">This wrapper was not obtained from a host context.</exception>
    /// <exception cref="HostingException">The native call failed.</exception>
    public void Load(ReadOnlySpan<byte> assembly, ReadOnlySpan<byte> symbols = default)
    {
        if (assembly.IsEmpty)
            throw new ArgumentException("Assembly bytes must not be empty.", nameof(assembly));

        if (_fn is null)
            throw new InvalidOperationException("This loader was not obtained from a host context.");

        fixed (byte* a = assembly)
        fixed (byte* s = symbols)
        {
            var rc = _fn(a, (nuint)assembly.Length, s, (nuint)symbols.Length, null, null);
            HostingError.ThrowIfFailed(rc, "load_assembly_bytes");
        }
    }

    /// <inheritdoc/>
    public bool Equals(AssemblyBytesLoader other)
    {
        return (nint)_fn == (nint)other._fn;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is AssemblyBytesLoader other && Equals(other);
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
    public static bool operator ==(AssemblyBytesLoader left, AssemblyBytesLoader right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two wrappers reference different function pointers.</summary>
    /// <param name="left">The first wrapper.</param>
    /// <param name="right">The second wrapper.</param>
    /// <returns><see langword="true"/> if not equal; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(AssemblyBytesLoader left, AssemblyBytesLoader right)
    {
        return !left.Equals(right);
    }
}
