# IronDB

A NoSQL database engine written in modern C# for .NET 10.

IronDB is an early-stage, work-in-progress document database. The current codebase
focuses on the low-level foundations a high-performance storage engine needs:
unmanaged memory management, a compact binary ("blittable") JSON document format,
buffer pooling, compression, and a TCP transport layer.

> **This is a personal learning project.** IronDB is not intended for production use.
> It's a way for me to learn how a high-performance document database is built by
> studying and re-implementing its foundational pieces.

> **Status:** Pre-alpha. The infrastructure layers are taking shape; there is not yet
> a runnable server, query language, or client API. Expect breaking changes.

## Acknowledgements

IronDB is heavily inspired by, and in many places derived from,
[**RavenDB**](https://github.com/ravendb/ravendb) and its Voron storage engine.
Much of the low-level infrastructure here — the blittable JSON document format,
`ByteString` and unmanaged memory handling, the VxSort vectorized sorting, low-memory
monitoring, and more — follows RavenDB's design and code closely as a learning exercise.
All credit for those original designs goes to Hibernating Rhinos and the RavenDB
contributors. RavenDB is dual-licensed (AGPL / commercial); please refer to the
[RavenDB repository](https://github.com/ravendb/ravendb) for its licensing terms.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- A 64-bit platform. Parts of the code use `unsafe` blocks, hardware intrinsics
  (AVX2 vectorized sorting), and platform-native calls (libsodium).

## Building

The solution uses the newer XML-based solution format (`.slnx`).

```sh
# Restore and build the whole solution
dotnet build IronDB.slnx

# Build in Release
dotnet build IronDB.slnx -c Release
```

Common build settings are centralized in `Directory.Build.props`:
`net10.0`, nullable reference types, implicit usings, latest C# language version,
and **warnings treated as errors**.

## Project layout

All projects live under `src/`:

| Project | Description |
| --- | --- |
| **IronDB.Common** | Small shared utilities — argument guards (`Ensure`), validators, and helpers used across the other projects. |
| **IronDB.BufferManagement** | Pooled buffer allocation: `BufferManager`, `BufferPool`, and `BufferPoolStream` for reusing byte buffers and reducing GC pressure. |
| **IronDB.Core** | The heart of the engine. Binary/bit manipulation, compression (LZ4, Zstd, variable-size & zig-zag encoding), the blittable JSON document format and parsers, arena/unmanaged memory allocators, low-memory monitoring, hashing, platform intrinsics, logging abstractions, and threading primitives. |
| **IronDB.Core.Server** | Server-side infrastructure: unmanaged `ByteString` types and allocators, async primitives (`AsyncGuard`, `AsyncQueue`, `AsyncManualResetEvent`), performance meters/metrics, NLog-based logging, and AVX2 vectorized sorting (VxSort). |
| **IronDB.StorageEngine** | The persistence layer. Currently contains compression headers and the storage/debugging report model (storage, journal, tree, and buffer reports). |
| **IronDB.Transport.Tcp** | TCP transport: connections (plain and SSL), message framing (length-prefix, with optional buffer pooling), formatting, connection monitoring, and a server listener. |

Dependency direction, roughly:

```
Common ──► BufferManagement ──► StorageEngine
                                     │
Core ◄───────────────────────────────┘
 ├── Core.Server
 └── Transport.Tcp
```

## Third-party libraries

- [Microsoft.IO.RecyclableMemoryStream](https://github.com/microsoft/Microsoft.IO.RecyclableMemoryStream) — pooled memory streams
- [Nito.AsyncEx](https://github.com/StephenCleary/AsyncEx) — async coordination primitives
- [Serilog](https://serilog.net/) / [NLog](https://nlog-project.org/) — logging

## License

Because IronDB is a derivative work of RavenDB — whose server/core code is licensed
under the GNU Affero General Public License v3.0 — IronDB is released under the same
[**AGPL-3.0**](LICENSE) license. See the [Acknowledgements](#acknowledgements) section
above for details.
