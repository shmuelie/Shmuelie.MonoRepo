# ![Shmuelie.Zio.Windows](Shmuelie.Zio.Windows-32.png) Shmuelie.Zio.Windows

![Nuget](https://img.shields.io/badge/NUGET-WIP-blue?style=for-the-badge)
[![Keep a Changelog](https://img.shields.io/badge/Keep%20a%20Changelog-1.0.0-F25D30?style=for-the-badge)](CHANGELOG.md)

Implementations of `IFileSystem` for [Zio][Zio] that are Windows specific.

- `LibraryFileSystem` exposes unmanaged resources in PE+ as a file system.
- `NamespaceFileSystem` exposes Shell Namespace in Windows as file systems.
- `FileSystemNamespace` exposes `IFileSystem` as a Shell Namespace.

[Zio]: https://www.nuget.org/packages/Zio