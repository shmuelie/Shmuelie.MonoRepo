// <copyright file="ResourceHelpers.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using static Windows.Win32.Constants;
using static Windows.Win32.PInvoke;

namespace Shmuelie.Native
{
    /// <summary>
    /// Helpers for working with Win32 resources.
    /// </summary>
    internal static class ResourceHelpers
    {
        public static IReadOnlyList<(string type, string name, ushort language)> GetResources(FreeLibrarySafeHandle module)
        {
            List<(string type, string name, ushort language)> resources = new List<(string type, string name, ushort language)>();
            GCHandle? handle = null;
            try
            {
                handle = GCHandle.Alloc(resources);
                if (EnumResourceTypes(
                    module,
                    GetResources,
                    GCHandle.ToIntPtr(handle.Value)))
                {
                    return resources;
                }
            }
            finally
            {
                handle?.Free();
            }

            throw new Win32Exception();
        }

        public static IReadOnlyList<string> GetResourceTypes(FreeLibrarySafeHandle module)
        {
            List<string> types = new List<string>();
            GCHandle? handle = null;
            try
            {
                handle = GCHandle.Alloc(types);
                if (EnumResourceTypesEx(
                    module,
                    GetResourceTypes,
                    GCHandle.ToIntPtr(handle.Value),
                    RESOURCE_ENUM_LN | RESOURCE_ENUM_MUI,
                    (ushort)LANG_NEUTRAL))
                {
                    return types;
                }
            }
            finally
            {
                handle?.Free();
            }

            throw new Win32Exception();
        }

        public static IReadOnlyList<string> GetResourceNames(FreeLibrarySafeHandle module, string type)
        {
            List<string> names = new List<string>();
            GCHandle? handle = null;
            try
            {
                handle = GCHandle.Alloc(names);
                if (EnumResourceNamesEx(
                    module,
                    type,
                    GetResourceNames,
                    GCHandle.ToIntPtr(handle.Value),
                    RESOURCE_ENUM_LN | RESOURCE_ENUM_MUI,
                    (ushort)LANG_NEUTRAL))
                {
                    return names;
                }
            }
            finally
            {
                handle?.Free();
            }

            throw new Win32Exception();
        }

        public static IReadOnlyList<ushort> GetResourceLanguageNames(FreeLibrarySafeHandle module, string type, string name)
        {
            List<ushort> languages = new List<ushort>();
            GCHandle? handle = null;
            try
            {
                handle = GCHandle.Alloc(languages);
                if (EnumResourceLanguagesEx(
                    module,
                    type,
                    name,
                    GetResourceLanguageNames,
                    GCHandle.ToIntPtr(handle.Value),
                    RESOURCE_ENUM_LN | RESOURCE_ENUM_MUI,
                    (ushort)LANG_NEUTRAL))
                {
                    return languages;
                }
            }
            finally
            {
                handle?.Free();
            }

            throw new Win32Exception();
        }

        private static BOOL GetResources(HINSTANCE hModule, PWSTR lpType, IntPtr lParam)
        {
            return EnumResourceNames(hModule, lpType, GetResources, lParam);
        }

        private static BOOL GetResources(HINSTANCE hModule, PCWSTR lpType, PWSTR lpName, IntPtr lParam)
        {
            return EnumResourceLanguages(hModule, lpType, lpName, GetResources, lParam);
        }

        private static BOOL GetResources(HINSTANCE hModule, PCWSTR lpType, PCWSTR lpName, ushort wLanguage, IntPtr lParam)
        {
            GCHandle handle = GCHandle.FromIntPtr(lParam);
            if (handle.Target is List<(string type, string name, ushort language)> resources)
            {
                resources.Add((lpType.ToString(), lpName.ToString(), wLanguage));
                return true;
            }

            return false;
        }

        private static BOOL GetResourceTypes(HINSTANCE hModule, PWSTR lpType, IntPtr lParam)
        {
            GCHandle handle = GCHandle.FromIntPtr(lParam);
            if (handle.Target is List<string> types)
            {
                types.Add(lpType.ToString());
                return true;
            }

            return false;
        }

        private static BOOL GetResourceNames(HINSTANCE hModule, PCWSTR lpType, PWSTR lpName, IntPtr lParam)
        {
            GCHandle handle = GCHandle.FromIntPtr(lParam);
            if (handle.Target is List<string> names)
            {
                names.Add(lpName.ToString());
                return true;
            }

            return false;
        }

        private static BOOL GetResourceLanguageNames(HINSTANCE hModule, PCWSTR lpType, PCWSTR lpName, ushort wLanguage, IntPtr lParam)
        {
            GCHandle handle = GCHandle.FromIntPtr(lParam);
            if (handle.Target is List<ushort> languages)
            {
                languages.Add(wLanguage);
                return true;
            }

            return false;
        }
    }
}
