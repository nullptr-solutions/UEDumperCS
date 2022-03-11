using UEDumperCS.Interop;
using UEDumperCS.Utils;

using System.Runtime.InteropServices;
using System;

namespace UEDumperCS.Engine.UE3
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TArray<T>
    {
        public nint Data;
        public int  Num;
        public int  Max;

        /// <summary>
        /// Reads a <typeparamref name="T"/> at index <paramref name="idx"/> from the array
        /// </summary>
        /// <param name="handle">A handle to the target with atleast read permissions.</param>
        /// <param name="idx">The object index.</param>
        /// <param name="deref">If the pointer should be derefed.</param>
        /// <param name="objectPtr">The object pointer.</param>
        public T Read(nint handle, int idx, bool deref, out nint objectPtr)
        {
            objectPtr = 0;

            var ptrData = this[idx];
            if (ptrData is 0)
                return default;

            if (deref)
                ptrData = Kernel32.ReadMemory<nint>(handle, ptrData);

            if (ptrData is 0)
                return default;

            return Kernel32.ReadMemory<T>(handle, objectPtr = ptrData);
        }

        /// <summary>
        /// Checks if the given index is withing the array bounds.
        /// </summary>
        /// <param name="idx">The index to check.</param>
        public bool IsValidIndex(int idx) => idx < Num && idx >= 0;

        /// <summary>
        /// Retrieves the pointer to an object by its index.
        /// </summary>
        /// <param name="idx">The object index.</param>
        public nint this[int idx] => IsValidIndex(idx) ? Data + (idx * MarshalCache<nint>.Size) : 0;
    }
}