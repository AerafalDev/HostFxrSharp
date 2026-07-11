# HostFxrSharp

[![NuGet](https://img.shields.io/nuget/v/HostFxrSharp.svg)](https://www.nuget.org/packages/HostFxrSharp)
[![Downloads](https://img.shields.io/nuget/dt/HostFxrSharp.svg)](https://www.nuget.org/packages/HostFxrSharp)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/AerafalDev/HostFxrSharp/blob/main/LICENSE)

A safe, idiomatic, **Native-AOT-compatible** managed wrapper over the native .NET hosting components
(`hostfxr` and `nethost`), for .NET 10. Locate and load `hostfxr`, initialize host contexts, start the
runtime, manage runtime properties, run managed apps, and load assemblies or resolve managed function
pointers — with structured exceptions and `SafeHandle`-based lifetimes. Runs on **Windows, Linux and macOS**.

```sh
dotnet add package HostFxrSharp
```

```csharp
using HostFxrSharp;

// Run a managed application through the native host.
using HostContext context = HostFxr.InitializeForCommandLine(["MyApp.dll", "--flag"]);
int exitCode = context.RunApp();
```

## Documentation

**[Guides & API →](https://aerafaldev.github.io/HostFxrSharp/)**

---

Part of the [HostFxrSharp](https://github.com/AerafalDev/HostFxrSharp) project ·
[MIT](https://github.com/AerafalDev/HostFxrSharp/blob/main/LICENSE) © Aerafal
