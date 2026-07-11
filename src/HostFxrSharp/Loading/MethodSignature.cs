namespace HostFxrSharp.Loading;

/// <summary>
/// Describes the signature of a managed method targeted by <c>load_assembly_and_get_function_pointer</c> or
/// <c>get_function_pointer</c>, hiding the native <c>delegate_type_name</c> string and the
/// <c>UNMANAGEDCALLERSONLY_METHOD</c> sentinel.
/// </summary>
/// <remarks>
/// <para>
/// The native sentinel <c>UNMANAGEDCALLERSONLY_METHOD</c> is <c>((const char_t*)-1)</c> (see
/// <c>docs/RESEARCH.md §6</c>). This library never exposes that raw cast; select
/// <see cref="UnmanagedCallersOnly"/> instead. <c>default(MethodSignature)</c> equals <see cref="Default"/>.
/// </para>
/// </remarks>
public readonly struct MethodSignature : IEquatable<MethodSignature>
{
    /// <summary>Gets the default component entry-point signature (<c>delegate_type_name == NULL</c>).</summary>
    public static MethodSignature Default =>
        new(MethodSignatureKind.ComponentEntryPoint, null);

    /// <summary>
    /// Gets the signature indicating the target method carries
    /// <see cref="System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute"/> (.NET 5+).
    /// </summary>
    public static MethodSignature UnmanagedCallersOnly =>
        new(MethodSignatureKind.UnmanagedCallersOnly, null);

    /// <summary>Gets the kind of signature description.</summary>
    public MethodSignatureKind Kind { get; }

    /// <summary>Gets the assembly-qualified delegate type name for <see cref="MethodSignatureKind.DelegateType"/>; otherwise <see langword="null"/>.</summary>
    public string? DelegateTypeName { get; }

    private MethodSignature(MethodSignatureKind kind, string? delegateTypeName)
    {
        Kind = kind;
        DelegateTypeName = delegateTypeName;
    }

    /// <summary>Creates a signature described by an assembly-qualified delegate type name.</summary>
    /// <param name="assemblyQualifiedDelegateTypeName">The assembly-qualified name of the delegate type describing the method signature.</param>
    /// <returns>A <see cref="MethodSignature"/> of kind <see cref="MethodSignatureKind.DelegateType"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="assemblyQualifiedDelegateTypeName"/> is <see langword="null"/> or empty.</exception>
    public static MethodSignature FromDelegateType(string assemblyQualifiedDelegateTypeName)
    {
        ArgumentException.ThrowIfNullOrEmpty(assemblyQualifiedDelegateTypeName);
        return new MethodSignature(MethodSignatureKind.DelegateType, assemblyQualifiedDelegateTypeName);
    }

    internal unsafe byte* ResolveNativePointer(byte* delegateTypeName)
    {
        return Kind switch
        {
            MethodSignatureKind.ComponentEntryPoint => null,
            MethodSignatureKind.UnmanagedCallersOnly => (byte*)-1,
            _ => delegateTypeName
        };
    }

    /// <inheritdoc/>
    public bool Equals(MethodSignature other)
    {
        return Kind == other.Kind && string.Equals(DelegateTypeName, other.DelegateTypeName, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is MethodSignature other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine((int)Kind, DelegateTypeName);
    }

    /// <summary>Determines whether two signatures are equal.</summary>
    /// <param name="left">The first signature.</param>
    /// <param name="right">The second signature.</param>
    /// <returns><see langword="true"/> if equal; otherwise <see langword="false"/>.</returns>
    public static bool operator ==(MethodSignature left, MethodSignature right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two signatures are not equal.</summary>
    /// <param name="left">The first signature.</param>
    /// <param name="right">The second signature.</param>
    /// <returns><see langword="true"/> if not equal; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(MethodSignature left, MethodSignature right)
    {
        return !left.Equals(right);
    }
}
