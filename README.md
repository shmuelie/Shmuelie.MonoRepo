# Shmuelie.MonoRepo

[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.1-4BAAAA?style=for-the-badge)](CODE_OF_CONDUCT.md)
[![Keep a Changelog](https://img.shields.io/badge/Keep%20a%20Changelog-1.0.0-F25D30?style=for-the-badge)](CHANGELOG.md)

Mono Repository for all .NET Projects. WIP projects may not exist in repo or Nuget.

## Shmuelie.Common

![Nuget](https://img.shields.io/badge/NUGET-WIP-blue?style=for-the-badge)

Classes and types that don't belong in a dedicated package.

## ![Shmuelie.MSBuild.Compression](Shmuelie.MSBuild.Compression/Shmuelie.MSBuild.Compression-24.png) Shmuelie.MSBuild.Compression

![Nuget](https://img.shields.io/badge/NUGET-WIP-blue?style=for-the-badge)

MSBuild tasks for created archives and extracting files from archives.

## ![Shmuelie.MSBuild.Publish](Shmuelie.MSBuild.Publish/Shmuelie.MSBuild.Publish-24.png) Shmuelie.MSBuild.Publish

![Nuget](https://img.shields.io/badge/NUGET-WIP-blue?style=for-the-badge)

MSBuild tasks for publishing NuGet Packages.

## ![Shmuelie.Zio.Glob](Shmuelie.Zio.Glob/Shmuelie.Zio.Glob-24.png) Shmuelie.Zio.Glob

![Nuget](https://img.shields.io/nuget/v/Shmuelie.Zio.Glob?style=for-the-badge)

Support for using [Microsoft.Extensions.FileSystemGlobbing][FileSystemGlobbing] with [Zio][Zio]'s File Systems.

## ![Shmuelie.Zio.FileSystems](Shmuelie.Zio.FileSystems/Shmuelie.Zio.FileSystems-24.png) Shmuelie.Zio.FileSystems

![Nuget](https://img.shields.io/nuget/v/Shmuelie.Zio.FileSystems?style=for-the-badge)

Additional implementations of `IFileSystem` for [Zio][Zio].

- `AssemblyFileSystem` exposes managed resources as a file system.
- `CompressedFileSystem` opens a `ZipArchive` as a file system.

## ![Shmuelie.Zio.Windows](Shmuelie.Zio.Windows/Shmuelie.Zio.Windows-24.png) Shmuelie.Zio.Windows

![Nuget](https://img.shields.io/badge/NUGET-WIP-blue?style=for-the-badge)

Implementations of `IFileSystem` for [Zio][Zio] that are Windows specific.

- `LibraryFileSystem` exposes unmanaged resources in PE+ as a file system.
- `NamespaceFileSystem` exposes Shell Namespace in Windows as file systems.
- `FileSystemNamespace` exposes `IFileSystem` as a Shell Namespace.

[FileSystemGlobbing]: https://www.nuget.org/packages/FileSystemGlobbing
[Zio]: https://www.nuget.org/packages/Zio
