using System.Runtime.InteropServices;
using HostFxrSharp.Interop.Methods;
using HostFxrSharp.Interop.Structs;
using HostFxrSharp.Loading;

namespace HostFxrSharp.Interop;

internal static unsafe class NativeHostingApi
{
    internal static int GetHostFxrPath(byte* buffer, ref nuint bufferSize, string? assemblyPath, string? dotnetRoot)
    {
        NativeLibraryResolver.EnsureInstalled();

        var size = bufferSize;
        var asm = NativeString.Allocate(assemblyPath);
        var dr = NativeString.Allocate(dotnetRoot);

        try
        {
            GetHostFxrParametersNative* pParams = null;

            if (assemblyPath is not null || dotnetRoot is not null)
            {
                GetHostFxrParametersNative parameters;
                parameters.Size = (nuint)sizeof(GetHostFxrParametersNative);
                parameters.AssemblyPath = asm;
                parameters.DotNetRoot = dr;
                pParams = &parameters;
            }

            var rc = NetHostMethods.GetHostFxrPath(buffer, &size, pParams);
            bufferSize = size;
            return rc;
        }
        finally
        {
            NativeString.Free(asm);
            NativeString.Free(dr);
        }
    }

    internal static int InitializeForCommandLine(ReadOnlySpan<string> args, string? hostPath, string? dotnetRoot, out nint handle)
    {
        NativeLibraryResolver.EnsureInstalled();

        handle = 0;
        var argc = args.Length;

        var argv = argc is 0 ? null : (byte**)NativeMemory.AllocZeroed((nuint)argc, (nuint)sizeof(nint));
        var hp = NativeString.Allocate(hostPath);
        var dr = NativeString.Allocate(dotnetRoot);

        try
        {
            for (var i = 0; i < argc; i++)
                argv[i] = NativeString.Allocate(args[i]);

            HostFxrInitializeParametersNative* pParams = null;

            if (hostPath is not null || dotnetRoot is not null)
            {
                HostFxrInitializeParametersNative parameters;
                parameters.Size = (nuint)sizeof(HostFxrInitializeParametersNative);
                parameters.HostPath = hp;
                parameters.DotNetRoot = dr;
                pParams = &parameters;
            }

            nint local = 0;
            var rc = HostFxrMethods.HostFxrInitializeForDotnetCommandLine(argc, argv, pParams, &local);
            handle = local;
            return rc;
        }
        finally
        {
            for (var i = 0; i < argc; i++)
                NativeString.Free(argv[i]);

            if (argv is not null)
                NativeMemory.Free(argv);

            NativeString.Free(hp);
            NativeString.Free(dr);
        }
    }

    internal static int InitializeForRuntimeConfig(string runtimeConfigPath, string? hostPath, string? dotnetRoot, out nint handle)
    {
        NativeLibraryResolver.EnsureInstalled();

        var path = NativeString.Allocate(runtimeConfigPath);
        var hp = NativeString.Allocate(hostPath);
        var dr = NativeString.Allocate(dotnetRoot);

        try
        {
            HostFxrInitializeParametersNative* pParams = null;

            if (hostPath is not null || dotnetRoot is not null)
            {
                HostFxrInitializeParametersNative parameters;
                parameters.Size = (nuint)sizeof(HostFxrInitializeParametersNative);
                parameters.HostPath = hp;
                parameters.DotNetRoot = dr;
                pParams = &parameters;
            }

            nint local = 0;
            var rc = HostFxrMethods.HostFxrInitializeForRuntimeConfig(path, pParams, &local);
            handle = local;
            return rc;
        }
        finally
        {
            NativeString.Free(path);
            NativeString.Free(hp);
            NativeString.Free(dr);
        }
    }

    internal static int GetRuntimePropertyValue(nint handle, string name, out string? value)
    {
        NativeLibraryResolver.EnsureInstalled();

        value = null;
        var n = NativeString.Allocate(name);

        try
        {
            byte* v = null;
            var rc = HostFxrMethods.HostFxrGetRuntimePropertyValue(handle, n, &v);

            if (((HostStatusCode)rc).IsSuccess() && v is not null)
                value = NativeString.Read(v);

            return rc;
        }
        finally
        {
            NativeString.Free(n);
        }
    }

    internal static int SetRuntimePropertyValue(nint handle, string name, string? value)
    {
        NativeLibraryResolver.EnsureInstalled();

        var n = NativeString.Allocate(name);
        var v = NativeString.Allocate(value);

        try
        {
            return HostFxrMethods.HostFxrSetRuntimePropertyValue(handle, n, v);
        }
        finally
        {
            NativeString.Free(n);
            NativeString.Free(v);
        }
    }

    internal static int GetRuntimeProperties(nint handle, ref nuint count, string[]? keys, string[]? values)
    {
        NativeLibraryResolver.EnsureInstalled();

        if (keys is null || values is null)
        {
            nuint c = 0;
            var rc0 = HostFxrMethods.HostFxrGetRuntimeProperties(handle, &c, null, null);
            count = c;
            return rc0;
        }

        var cap = Math.Min(keys.Length, values.Length);
        var keyPtrs = cap is 0 ? [] : new nint[cap];
        var valuePtrs = cap is 0 ? [] : new nint[cap];
        var used = (nuint)cap;
        int rc;

        fixed (nint* kp = keyPtrs)
        fixed (nint* vp = valuePtrs)
            rc = HostFxrMethods.HostFxrGetRuntimeProperties(handle, &used, (byte**)kp, (byte**)vp);

        count = used;

        if (((HostStatusCode)rc).IsSuccess())
        {
            var n = (int)Math.Min(used, (nuint)cap);

            for (var i = 0; i < n; i++)
            {
                keys[i] = NativeString.Read((byte*)keyPtrs[i]) ?? string.Empty;
                values[i] = NativeString.Read((byte*)valuePtrs[i]) ?? string.Empty;
            }
        }

        return rc;
    }

    internal static int RunApp(nint handle)
    {
        NativeLibraryResolver.EnsureInstalled();
        return HostFxrMethods.HostFxrRunApp(handle);
    }

    internal static int GetRuntimeDelegate(nint handle, HostFxrDelegateType type, out nint result)
    {
        NativeLibraryResolver.EnsureInstalled();

        var d = (void*)nint.Zero;
        var rc = HostFxrMethods.HostFxrGetRuntimeDelegate(handle, (int)type, &d);
        result = (nint)d;
        return rc;
    }

    public static int Close(nint handle)
    {
        NativeLibraryResolver.EnsureInstalled();

        return HostFxrMethods.HostFxrClose(handle);
    }
}
