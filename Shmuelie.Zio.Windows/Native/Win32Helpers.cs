// <copyright file="Win32Helpers.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using static Windows.Win32.Constants;
using static Windows.Win32.PInvoke;

namespace Shmuelie.Native
{
    internal static class Win32Helpers
    {
        public const int MaxPathSegmentLength = 255;

        public const int MaxPathLength = 259;

        public const int MaxExtendedPathLength = 0x7FFF - 24;

        public const int StackBufferLength = 256;

        public static string AdaptFixedSizeToAllocatedResult(Func<char[], int> callback, bool includesNull = true)
        {
            int mod = includesNull ? 1 : 0;
            char[] value = new char[StackBufferLength];
            value[0] = '\0';
            int valueLengthNeededWithNull = callback(value);
            if (valueLengthNeededWithNull <= value.Length)
            {
                return new string(value, 0, valueLengthNeededWithNull - mod);
            }

            value = new char[valueLengthNeededWithNull];
            int secondLength = callback(value);
            Debug.Assert(valueLengthNeededWithNull == secondLength, "Size is not correct.");
            return new string(value, 0, secondLength - mod);
        }

        public static unsafe string GetLanguageName(ushort wLang) => AdaptFixedSizeToAllocatedResult(ch =>
        {
            fixed (char* c = &ch[0])
            {
                PWSTR szLang = new PWSTR(c);
                return (int)VerLanguageName(wLang, szLang, (uint)ch.Length);
            }
        });

        public static unsafe IReadOnlyList<(string name, ushort id)> GetLanguages()
        {
            List<(string name, ushort id)> langauges = new List<(string name, ushort id)>();
            GCHandle? handle = null;
            try
            {
                handle = GCHandle.Alloc(langauges);
                if (EnumUILanguages(
                    GetLanguages,
                    MUI_LANGUAGE_ID,
                    GCHandle.ToIntPtr(handle.Value)))
                {
                    return langauges;
                }
            }
            finally
            {
                handle?.Free();
            }

            throw new Win32Exception();
        }

        private static BOOL GetLanguages(PWSTR param0, IntPtr param1)
        {
            GCHandle handle = GCHandle.FromIntPtr(param1);
            if (handle.Target is List<(string name, ushort id)> languages && ushort.TryParse(param0.AsSpan(), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ushort wLanguage))
            {
                languages.Add((GetLanguageName(wLanguage), wLanguage));
                return true;
            }

            return false;
        }
    }
}
