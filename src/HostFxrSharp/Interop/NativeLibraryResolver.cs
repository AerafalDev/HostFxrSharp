using System.Reflection;
using System.Runtime.InteropServices;
using HostFxrSharp.Interop.Methods;

namespace HostFxrSharp.Interop;

internal static class NativeLibraryResolver
{
    private static nint _hostFxrHandle;
    private static int _installed;

    internal static void EnsureInstalled()
    {
        if (Interlocked.Exchange(ref _installed, 1) is 0)
            NativeLibrary.SetDllImportResolver(typeof(NativeLibraryResolver).Assembly, Resolve);
    }

    internal static void SetHostFxrHandle(nint handle)
    {
        Volatile.Write(ref _hostFxrHandle, handle);
    }

    private static nint Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (string.Equals(libraryName, HostFxrMethods.LibraryName, StringComparison.OrdinalIgnoreCase))
            return Volatile.Read(ref _hostFxrHandle);

        if (string.Equals(libraryName, NetHostMethods.LibraryName, StringComparison.OrdinalIgnoreCase))
            return ResolveNetHost();

        return nint.Zero;
    }

    private static nint ResolveNetHost()
    {
        foreach (var candidate in EnumerateNetHostCandidates())
        {
            if (!string.IsNullOrEmpty(candidate) && File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out var handle))
                return handle;
        }

        return nint.Zero;
    }

    private static IEnumerable<string> EnumerateNetHostCandidates()
    {
        var fileName = GetNativeLibraryFileName(NetHostMethods.LibraryName);
        var baseDir = AppContext.BaseDirectory;

        yield return Path.Combine(baseDir, fileName);

        foreach (var rid in GetRidCandidates())
        {
            yield return Path.Combine(baseDir, "runtimes", rid, "native", fileName);

            if (TryFindInAppHostPack(rid, fileName) is { } packCandidate)
                yield return packCandidate;
        }
    }

    private static string? TryFindInAppHostPack(string rid, string fileName)
    {
        var runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
        var dotnetRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(runtimeDir))));

        if (string.IsNullOrEmpty(dotnetRoot))
            return null;

        var packDir = Path.Combine(dotnetRoot, "packs", $"Microsoft.NETCore.App.Host.{rid}");

        if (!Directory.Exists(packDir))
            return null;

        Version? best = null;
        string? bestPath = null;

        foreach (var versionDir in Directory.EnumerateDirectories(packDir))
        {
            if (!Version.TryParse(Path.GetFileName(versionDir), out var version))
                continue;

            var candidate = Path.Combine(versionDir, "runtimes", rid, "native", fileName);

            if (File.Exists(candidate) && (best is null || version > best))
            {
                best = version;
                bestPath = candidate;
            }
        }

        return bestPath;
    }

    internal static string GetNativeLibraryFileName(string baseName)
    {
        if (OperatingSystem.IsWindows())
            return $"{baseName}.dll";

        if (OperatingSystem.IsMacOS())
            return $"lib{baseName}.dylib";

        return $"lib{baseName}.so";
    }

    internal static string GetRid()
    {
        var os = OperatingSystem.IsWindows() ? "win" : OperatingSystem.IsMacOS() ? "osx" : "linux";

        var arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "x64"
        };

        return $"{os}-{arch}";
    }

    private static IEnumerable<string> GetRidCandidates()
    {
        var rid = GetRid();

        yield return rid;

        if (OperatingSystem.IsLinux())
            yield return rid.Replace("linux-", "linux-musl-", StringComparison.Ordinal);
    }
}
