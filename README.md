# Shmuelie.MonoRepo

[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.1-4BAAAA?style=for-the-badge)](CODE_OF_CONDUCT.md)

Mono Repository for all .NET Projects

## Shmuelie.Common

![Nuget](https://img.shields.io/badge/NUGET-WIP-blue?style=for-the-badge)

Classes and types that don't belong in a dedicated package.

## Shmuelie.MSBuild

Extra tasks for MSBuild

### ![Shmuelie.MSBuild.SharpZipLib](Shmuelie.MSBuild.SharpZipLib/Shmuelie.MSBuild.SharpZipLib-24.png) Shmuelie.MSBuild.SharpZipLib

![Nuget](https://img.shields.io/badge/NUGET-WIP-blue?style=for-the-badge)

## Shmuelie.Zio

Add-ons or Extensions to [Zio][Zio]. WIP projects may not exist in repo or Nuget.

### ![Shmuelie.Zio.Glob](Shmuelie.Zio.Glob/Shmuelie.Zio.Glob-24.png) Shmuelie.Zio.Glob

![Nuget](https://img.shields.io/nuget/v/Shmuelie.Zio.Glob?style=for-the-badge)

Support for using [Microsoft.Extensions.FileSystemGlobbing][FileSystemGlobbing] with Zio's File Systems.

### ![Shmuelie.Zio.FileSystems](Shmuelie.Zio.FileSystems/Shmuelie.Zio.FileSystems-24.png) Shmuelie.Zio.FileSystems

![Nuget](https://img.shields.io/nuget/v/Shmuelie.Zio.FileSystems?style=for-the-badge)

Additional implementations of `IFileSystem` for Zio.

- `AssemblyFileSystem` exposes managed resources as a file system.
- `CompressedFileSystem` opens a `ZipArchive` as a file system.

### ![Shmuelie.Zio.Network](Shmuelie.Zio.Network/Shmuelie.Zio.Network-24.png) Shmuelie.Zio.Network

![Nuget](https://img.shields.io/badge/NUGET-WIP-blue?style=for-the-badge)

Implementations of `IFileSystem` for networking for Zio.

- `NetworkFileSystem` adds support for NFS.
- `SmbFileSystem` adds support for SMB/CIFS.
### ![Shmuelie.Zio.Windows](Shmuelie.Zio.Windows/Shmuelie.Zio.Windows-24.png) Shmuelie.Zio.Windows

![Nuget](https://img.shields.io/badge/NUGET-WIP-blue?style=for-the-badge)

Implementations of `IFileSystem` that are Windows specific.

- `LibraryFileSystem` exposes unmanaged resources in PE+ as a file system.
- `NamespaceFileSystem` exposes Shell Namespace in Windows as file systems.
- `FileSystemNamespace` exposes `IFileSystem` as a Shell Namespace.

### ![Shmuelie.Zio.DiskUtils](Shmuelie.Zio.DiskUtils/Shmuelie.Zio.DiskUtils-24.png) Shmuelie.Zio.DiskUtils

![Nuget](https://img.shields.io/badge/NUGET-WIP-blue?style=for-the-badge)

Support for using [DiskUtils][DiskUtils] with Zio. Supports using either as the source for the other.

[FileSystemGlobbing]: https://www.nuget.org/packages/FileSystemGlobbing
[Zio]: https://www.nuget.org/packages/Zio
[DiskUtils]: https://github.com/DiscUtils/DiscUtils