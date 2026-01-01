# Storage

[![Build-Deploy](https://github.com/datafac/storage/actions/workflows/dotnet.yml/badge.svg)](https://github.com/datafac/storage/actions/workflows/dotnet.yml)
![NuGet Version](https://img.shields.io/nuget/v/DataFac.Storage)
![NuGet Downloads](https://img.shields.io/nuget/dt/DataFac.Storage)
![GitHub License](https://img.shields.io/github/license/Datafac/storage)
![GitHub Sponsors](https://img.shields.io/github/sponsors/psiman62)

Named blob storage abstractions and implementations.

*Note: V2.0 has breaking changes. The use of ReadOnlyMemory\<byte\> parameters in public APIs has been
replaced with ReadOnlySequence\<byte\>. This change is primarily for performance reasons - part of the ongoing
effort to remove unnecessary memory allocations.*

## How to sponsor
If you find this package useful, please consider sponsoring my work on GitHub 
at https://github.com/sponsors/Psiman62
or buy me a coffee at https://www.buymeacoffee.com/psiman62
