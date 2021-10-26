// <copyright file="EnumResource.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using Windows.Win32;
using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace Shmuelie.Zio.Windows.Benchmark
{
    public class EnumResource
    {
        private readonly FreeLibrarySafeHandle lib = PInvoke.LoadLibrary(@"C:\Windows\explorer.exe");

        public delegate void ListAdd(ushort value);

        [Benchmark]
        public List<ushort> Lambda()
        {
            List<ushort> l = new List<ushort>();
            PInvoke.EnumResourceTypes(lib, (HINSTANCE instance, PWSTR lpType, IntPtr __) =>
            {
                return PInvoke.EnumResourceNames(instance, lpType, (HINSTANCE ___, PCWSTR ____, PWSTR lpName, IntPtr _____) =>
                {
                    return PInvoke.EnumResourceLanguages(instance, lpType, lpName, (HINSTANCE ______, PCWSTR _______, PCWSTR ________, ushort wLanguage, IntPtr _________) =>
                    {
                        l.Add(wLanguage);
                        return true;
                    }, IntPtr.Zero);
                }, IntPtr.Zero);
            }, IntPtr.Zero);
            return l;
        }

        [Benchmark]
        public List<ushort> FunctionPointer()
        {
            List<ushort> l = new List<ushort>();
            ListAdd add = l.Add;
            IntPtr addPointer = Marshal.GetFunctionPointerForDelegate(add);
            PInvoke.EnumResourceTypes(lib, FunctionPointerCallback, addPointer);
            GC.KeepAlive(add);
            return l;
        }

        private static BOOL FunctionPointerCallback(HINSTANCE hModule, PWSTR lpType, IntPtr lParam)
        {
            return PInvoke.EnumResourceNames(hModule, lpType, FunctionPointerCallback, lParam);
        }

        private static BOOL FunctionPointerCallback(HINSTANCE hModule, PCWSTR lpType, PWSTR lpName, IntPtr lParam)
        {
            return PInvoke.EnumResourceLanguages(hModule, lpType, lpName, FunctionPointerCallback, lParam);
        }

        private static BOOL FunctionPointerCallback(HINSTANCE hModule, PCWSTR lpType, PCWSTR lpName, ushort wLanguage, IntPtr lParam)
        {
            ListAdd add = Marshal.GetDelegateForFunctionPointer<ListAdd>(lParam);
            add(wLanguage);
            return true;
        }

        [Benchmark]
        public List<ushort> PinObject()
        {
            List<ushort> l = new List<ushort>();
            GCHandle handle = GCHandle.Alloc(l);
            PInvoke.EnumResourceTypes(lib, PinObjectCallback, GCHandle.ToIntPtr(handle));
            handle.Free();
            return l;
        }

        private static BOOL PinObjectCallback(HINSTANCE hModule, PWSTR lpType, IntPtr lParam)
        {
            return PInvoke.EnumResourceNames(hModule, lpType, PinObjectCallback, lParam);
        }

        private static BOOL PinObjectCallback(HINSTANCE hModule, PCWSTR lpType, PWSTR lpName, IntPtr lParam)
        {
            return PInvoke.EnumResourceLanguages(hModule, lpType, lpName, PinObjectCallback, lParam);
        }

        private static BOOL PinObjectCallback(HINSTANCE hModule, PCWSTR lpType, PCWSTR lpName, ushort wLanguage, IntPtr lParam)
        {
            GCHandle handle = GCHandle.FromIntPtr(lParam);
            List<ushort>? l = handle.Target as List<ushort>;
            l?.Add(wLanguage);
            return true;
        }
    }
}
