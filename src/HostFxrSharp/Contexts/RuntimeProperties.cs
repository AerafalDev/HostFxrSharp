using HostFxrSharp.Exceptions;

namespace HostFxrSharp.Contexts;

/// <summary>Typed accessor for a host context's runtime properties.</summary>
/// <remarks>
/// Reads are always allowed (until disposal). Modification via <see cref="Set(string, string?)"/> is only legal
/// on the <see cref="HostContextKind.First"/> context and only before the runtime has started; otherwise an
/// <see cref="InvalidOperationException"/> is thrown. See <see cref="CanModify"/>.
/// </remarks>
public sealed class RuntimeProperties
{
    private readonly HostContext _context;

    /// <summary>Gets a value indicating whether properties can currently be modified.</summary>
    public bool CanModify =>
        _context.CanModifyRuntimeProperties;

    /// <summary>Gets the value of a runtime property.</summary>
    /// <param name="name">The property name.</param>
    /// <returns>The property value.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ObjectDisposedException">The owning context has been disposed.</exception>
    /// <exception cref="RuntimePropertyNotFoundException">The property does not exist.</exception>
    public string this[string name] =>
        _context.GetRuntimeProperty(name);

    internal RuntimeProperties(HostContext context)
    {
        _context = context;
    }

    /// <summary>Attempts to get the value of a runtime property.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The property value if found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the property exists; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ObjectDisposedException">The owning context has been disposed.</exception>
    public bool TryGetValue(string name, out string? value)
    {
        return _context.TryGetRuntimeProperty(name, out value);
    }

    /// <summary>Sets (or, with a <see langword="null"/> value, removes) a runtime property.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The value to set, or <see langword="null"/> to remove the property.</param>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ObjectDisposedException">The owning context has been disposed.</exception>
    /// <exception cref="InvalidOperationException">The context is secondary, or the runtime has already started.</exception>
    /// <exception cref="HostingException">The native call failed.</exception>
    public void Set(string name, string? value)
    {
        _context.SetRuntimeProperty(name, value);
    }

    /// <summary>Gets all runtime properties for this context as an immutable snapshot.</summary>
    /// <returns>A dictionary of property name/value pairs (ordinal key comparison).</returns>
    /// <exception cref="ObjectDisposedException">The owning context has been disposed.</exception>
    public IReadOnlyDictionary<string, string> GetAll()
    {
        return _context.GetAllRuntimeProperties();
    }
}
