using System.Runtime.InteropServices;

namespace HostFxrSharp.TestComponent;

/// <summary>
/// Entry points exercised by the host integration tests. Each shape corresponds to one
/// <c>MethodSignature</c> kind understood by <c>load_assembly_and_get_function_pointer</c>.
/// </summary>
public static class Exports
{
    /// <summary>Unmanaged-callers-only entry point. Invoked as <c>delegate* unmanaged&lt;int, int, int&gt;</c>.</summary>
    [UnmanagedCallersOnly]
    public static int Add(int a, int b)
    {
        return a + b;
    }

    /// <summary>Default component entry point shape: <c>int (nint arg, int argSizeInBytes)</c>.</summary>
    public static int ComponentEntry(nint arg, int argSizeInBytes)
    {
        return 42;
    }

    /// <summary>Target for the delegate-typed signature (<see cref="ComputeDelegate"/>).</summary>
    public static int Square(int value)
    {
        return value * value;
    }
}

/// <summary>Delegate type used to resolve <see cref="Exports.Square"/> via <c>MethodSignature.FromDelegateType</c>.</summary>
/// <param name="value">The input value.</param>
/// <returns>The computed result.</returns>
public delegate int ComputeDelegate(int value);
