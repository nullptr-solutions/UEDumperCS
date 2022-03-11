using UEDumperCS.Interop;

using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace UEDumperCS.Utils
{
    public static class PatternScanner
    {
        /// <summary>
        /// Parses a pattern to a byte array.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="offset">The offset.</param>
        static List<byte?> ParsePattern(string pattern, out int offset)
        {
            offset = 0x0;

            var patternSplit = pattern.Split();
            var patternBytes = new List<byte?>();

            for (var pairIdx = 0; pairIdx < patternSplit.Length; pairIdx++)
            {
                var pair = patternSplit[pairIdx];
                if (pair.StartsWith("[") || pair.StartsWith("("))
                {
                    offset = patternBytes.Count;

                    for (var i = 1; i < pair.Length - 1; i++)
                        patternBytes.Add(null);
                }
                else
                {
                    if (pair.StartsWith("."))
                    {
                        for (var i = 0; i < pair.Length; i++)
                            patternBytes.Add(null);

                        continue;
                    }

                    patternBytes.Add(pair.Contains("?") ? null : Convert.ToByte(pair, 16));
                }
            }

            return patternBytes;
        }

        /// <summary>
        /// Searches for a byte <paramref name="pattern"/> in <paramref name="module"/> with <paramref name="handle"/>.
        /// </summary>
        /// <param name="handle">The handle to be used for reading memory.</param>
        /// <param name="module">The module to search the pattern in.</param>
        /// <param name="pattern">The byte pattern to search for.</param>
        /// <param name="exp">The expression used for additional operations.</param>
        public static nint Search(nint handle, ProcessModule module, string pattern, Func<byte[], int, int> exp = null)
        {
            var patternBytes = ParsePattern(pattern, out var patternOffset);
            if (patternBytes.Count is 0)
                return 0;

            var modBuf = Kernel32.ReadMemory(handle, module.BaseAddress, module.ModuleMemorySize);
            if (modBuf is null || modBuf.Length is 0)
                return 0;

            var result = 0;
            for (var i = 0; i < modBuf.Length; i++)
            {
                var found = true;
                for (var j = 0; j < patternBytes.Count; j++)
                {
                    if (patternBytes[j] is null || modBuf[i + j] == patternBytes[j])
                        continue;

                    found = false;
                    break;
                }

                if (found)
                {
                    result = i + patternOffset;
                    break;
                }
            }

            return result > 0
                ? IntPtr.Add(module.BaseAddress, exp is null
                    ? result
                    : exp.Invoke(modBuf, result))
                : 0;
        }
    }
}