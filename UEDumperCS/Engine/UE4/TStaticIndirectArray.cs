using UEDumperCS.Interop;
using UEDumperCS.Utils;

using System.Runtime.InteropServices;
using System;

namespace UEDumperCS.Engine.UE4
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TStaticIndirectArray<T>
    {
        const int MAX_TOTAL_ELEMENTS = 2 * 1024 * 1024;
        const int ELEMENTS_PER_CHUNK = 16384;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (MAX_TOTAL_ELEMENTS + ELEMENTS_PER_CHUNK - 1) / ELEMENTS_PER_CHUNK)]
        public readonly nint[] Chunks;
        public readonly int    NumElements;
        public readonly int    NumChunks;

        /// <summary>
        /// Checks if the given index is withing the array bounds.
        /// </summary>
        /// <param name="idx">The index to check.</param>
        public bool IsValidIndex(int idx) => idx < NumElements && idx >= 0;

        /// <summary>
        /// Retrieves a <see cref="FNameEntry"/> with the specified <paramref name="idx"/> from the <see cref="Chunks"/>.
        /// </summary>
        /// <param name="handle">A handle to the target with atleast read permissions.</param>
        /// <param name="idx">The <see cref="FNameEntry"/>'s index.</param>
        public T GetById(nint handle, int idx)
        {
            if (idx > NumElements)
                return default;

            var chunkIdx = idx / ELEMENTS_PER_CHUNK;
            if (chunkIdx > Chunks.Length)
                return default;

            var chunkAddress = Chunks[chunkIdx];
            if (chunkAddress is 0)
                return default;

            var objectAddress = Kernel32.ReadMemory<nint>(handle, chunkAddress + (idx % ELEMENTS_PER_CHUNK * MarshalCache<nint>.Size));
            if (objectAddress > 0)
                return Kernel32.ReadMemory<T>(handle, objectAddress);

            return default;
        }
    }
}
