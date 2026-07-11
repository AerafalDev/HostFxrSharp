namespace HostFxrSharp.Loading;

/// <summary>How the target managed method's signature is described to the runtime helper.</summary>
public enum MethodSignatureKind
{
    /// <summary>
    /// The default <c>ComponentEntryPoint</c> signature (<c>delegate_type_name == NULL</c>):
    /// <c>int (IntPtr args, int sizeBytes)</c> mapping to native <c>int(void*, int32_t)</c>.
    /// </summary>
    ComponentEntryPoint,

    /// <summary>
    /// The managed method is marked with <see cref="System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute"/>.
    /// Corresponds to the native <c>UNMANAGEDCALLERSONLY_METHOD</c> sentinel (<c>.NET 5+</c>).
    /// </summary>
    UnmanagedCallersOnly,

    /// <summary>The signature is given by an assembly-qualified delegate type name.</summary>
    DelegateType
}
