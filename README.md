# HostFxrSharp

[![Build](https://github.com/AerafalDev/HostFxrSharp/actions/workflows/ci.yml/badge.svg)](https://github.com/AerafalDev/HostFxrSharp/actions/workflows/ci.yml)
[![Docs](https://github.com/AerafalDev/HostFxrSharp/actions/workflows/docs.yml/badge.svg)](https://aerafaldev.github.io/HostFxrSharp/)
[![NuGet](https://img.shields.io/nuget/v/HostFxrSharp.svg)](https://www.nuget.org/packages/HostFxrSharp)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A safe, idiomatic, **Native-AOT-compatible** managed wrapper over the native .NET hosting components
(`hostfxr` and `nethost`). Locate and load `hostfxr`, initialize host contexts, start the runtime, read
and write runtime properties, run managed apps, and load assemblies or resolve managed function pointers,
all from clean C#, on **.NET 10**, across **Windows, Linux and macOS**.

Everything goes through source-generated P/Invokes and unmanaged function pointers (the only managed→native
callback uses an `[UnmanagedCallersOnly]` trampoline), so the whole surface stays Native-AOT friendly. Native
strings are marshalled per-platform (`char_t` is UTF-16 on Windows, UTF-8 on Unix).

## Documentation

**[aerafaldev.github.io/HostFxrSharp](https://aerafaldev.github.io/HostFxrSharp/)** — guides for locating and
loading `hostfxr`, host contexts, runtime properties, and loading assemblies / function pointers.

## Packages

| Package | Version | Description |
| --- | --- | --- |
| [**HostFxrSharp**](src/HostFxrSharp/README.md) | [![NuGet](https://img.shields.io/nuget/v/HostFxrSharp.svg)](https://www.nuget.org/packages/HostFxrSharp) | Managed wrapper over `hostfxr` and `nethost`: locate/load the host, start the runtime, run apps, load assemblies and resolve function pointers. |

```sh
dotnet add package HostFxrSharp
```

## Quick start

```csharp
using HostFxrSharp;

// Run a managed application through the native host — works the same on Windows, Linux and macOS.
using HostContext context = HostFxr.InitializeForCommandLine(["MyApp.dll", "--flag"]);
int exitCode = context.RunApp();
```

Or locate `hostfxr` and get a native function pointer to a managed static method:

```csharp
using HostFxrSharp;

string hostFxrPath = NetHost.GetHostFxrPath();

using HostContext context = HostFxr.InitializeForRuntimeConfig("MyComponent.runtimeconfig.json");
var loader = context.GetAssemblyFunctionPointerLoader();

nint entryPoint = loader.Load(
    assemblyPath: "MyComponent.dll",
    typeName: "MyComponent.Entry, MyComponent",
    methodName: "Run",
    signature: MethodSignature.UnmanagedCallersOnly);
```

## Contributing

Issues and pull requests are welcome — see [CONTRIBUTING](CONTRIBUTING.md).

## License

[MIT](LICENSE) © Aerafal
