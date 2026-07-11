# Contributing to HostFxrSharp

Thanks for taking the time to contribute! This project is a managed wrapper over the native .NET hosting
components (`hostfxr` / `nethost`). Issues, bug reports and pull requests are all welcome.

By participating you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md).

## Getting started

You need the **.NET 10 SDK**. The exact band is pinned in [`global.json`](global.json), so the right SDK is
selected automatically.

```sh
git clone https://github.com/AerafalDev/HostFxrSharp.git
cd HostFxrSharp

dotnet build -c Release        # build the library and tests
dotnet test  -c Release        # run the test suite
```

## Cross-platform first

The hosting APIs use `pal::char_t`, which is **UTF-16 on Windows** but **UTF-8 on Linux and macOS**, and the
native libraries have different names (`nethost.dll`, `libnethost.so`, `libnethost.dylib`). Because of this:

- **Never** marshal a native string as if it were always UTF-16. Route every native string through
  `NativeString` (which encodes/decodes per platform) — never `Marshal.*Uni` or `new string(char*)` directly.
- Native library file names and RIDs go through `NativeLibraryResolver`; add new OS/architecture handling there.
- CI builds and tests on **Windows, Linux and macOS** — a change is not done until the matrix is green.

## Coding conventions

Style is enforced by [`.editorconfig`](.editorconfig); please don't fight it. The points that matter most here:

- **C# style** — file-scoped namespaces, 4-space indentation, Allman braces, `var` when the type is apparent,
  `_camelCase` private fields.
- **XML documentation** — every public type and member carries `///` docs; the build generates the XML doc
  file, so a missing comment on public API is a build error. Explain the *native* behaviour being wrapped.
- **No primary constructors** — declare constructors explicitly.
- **Member order** — fields, then properties, then constructors, then methods.
- **Interop hygiene** — all native access uses source-generated `[LibraryImport]` P/Invokes or unmanaged
  function pointers (no reflection, no runtime marshalling of delegates) so the surface stays Native-AOT safe.
  Native handles are owned by `SafeHandle`s.
- **Warnings are errors** — the build runs with `TreatWarningsAsErrors`, so a green build means zero warnings.

Files are **UTF-8 (no BOM) with CRLF** line endings, normalized by `.gitattributes`.

## Tests

New behaviour ships with a test. The suite lives in `src/HostFxrSharp.Tests` (xUnit). Prefer tests that are
deterministic on every OS — for example, verifying that native strings round-trip through their platform
encoding, or that the resolver picks the right library name and RID for the current platform.

Run `dotnet test -c Release` before opening a pull request.

## Pull requests

- Branch off `main`; keep each change small and self-contained.
- Write clear, present-tense commit messages — one logical change per commit.
- Make sure `dotnet build -c Release` and `dotnet test -c Release` are green.
- Describe *what* changed and *why*. When you touch interop, cite the relevant hosting API or native header.

## Reporting bugs

Open an issue with the OS and architecture, the .NET version, what you expected and what happened, and a
minimal repro if you can share one. For security-sensitive reports, follow the [Security Policy](SECURITY.md)
instead of opening a public issue.
