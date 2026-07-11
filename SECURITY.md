# Security Policy

## Supported versions

HostFxrSharp is published as a v1.0.0 library and is developed on the `main` branch. Security fixes are
applied to `main` and shipped in the next release; only the **latest published version** is supported.

| Version | Supported |
| --- | --- |
| Latest release | ✅ |
| Older releases | ❌ |

## Reporting a vulnerability

**Please do not report security issues in public GitHub issues or pull requests.**

This library loads and calls into native code (`nethost` / `hostfxr` / `hostpolicy`) and can start a .NET
runtime in the current process. The security surface that matters most is therefore **how native libraries
are located and loaded** — for example a resolution path that could load an unexpected `nethost`/`hostfxr`,
mishandling of caller-supplied paths (`DOTNET_ROOT`, host path, assembly path), or memory-safety issues in the
interop marshalling layer.

Report privately through either channel:

- **GitHub Security Advisories** — open the repository's **Security → Report a vulnerability** tab to start a
  private advisory (preferred).
- **Email** — <aerafal.github@gmail.com>.

Please include:

- a description of the issue and its impact,
- a minimal repro (OS, architecture, .NET version, and the smallest code/inputs that trigger it),
- the version (or commit) you observed it on.

## What to expect

- We aim to acknowledge a report within a few days.
- We'll confirm the issue, keep you updated as we work on a fix, and credit you in the release notes unless you
  prefer to stay anonymous.
- Once a fix is released, the advisory is published.

Thank you for helping keep HostFxrSharp and its users safe.
