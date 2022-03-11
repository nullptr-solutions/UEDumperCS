using UEDumperCS.Interop;
using UEDumperCS.Utils;

using System.Runtime.InteropServices;
using System;

namespace UEDumperCS_Borderlands3.Engine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FChunkedFixedUObjectArray<T>
    {
        public readonly Remote<nint> ChunkTable;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x8)]
        readonly byte[]              pad_0008;
        public readonly int          MaxElements;
        public readonly int          NumElements;
        public readonly int          MaxChunks;
        public readonly int          NumChunks;

        /// <summary>
        /// Checks if the given index is withing the array bounds.
        /// </summary>
        /// <param name="idx">The index to check.</param>
        public bool IsValidIndex(int idx) => idx < NumElements && idx >= 0;

        /// <summary>
        /// Retrieves a <see cref="T"/> with the specified <paramref name="idx"/> from the <see cref="ChunkTable"/>.
        /// </summary>
        /// <param name="handle">A handle to the target process with atleast read permissions.</param>
        /// <param name="idx">The <see cref="T"/>'s index.</param>
        public T GetById(nint handle, int idx)
        {
            const int NUM_ELEMENTS_PER_CHUNK = 65 * 1024;

            if (idx > NumElements)
                return default;

            var chunkIdx = idx / NUM_ELEMENTS_PER_CHUNK;
            if (chunkIdx >= NumChunks)
                return default;

            // INFO:
            // - can be optimized by caching chunk table addresses
            var chunkAddress = ChunkTable.Read(handle, chunkIdx * MarshalCache<nint>.Size);
            if (chunkAddress <= 0)
                return default;

            return Kernel32.ReadMemory<T>(handle, chunkAddress + (idx % NUM_ELEMENTS_PER_CHUNK * MarshalCache<T>.Size));
        }
    }
}