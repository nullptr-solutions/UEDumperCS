using UEDumperCS.Interop;

using System.Runtime.InteropServices;
using System;

namespace UEDumperCS.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Remote<T>
    {
        /// <summary>
        /// Holds the address to the data.
        /// </summary>
        public nint Pointer;

        /// <summary>
        /// Checks if the <see cref="Pointer"/> is valid.
        /// </summary>
        public bool IsValid => Pointer > 0;

        /// <summary>
        /// Reads data at the <see cref="Pointer"/> address and converts it to <see cref="T"/>.
        /// </summary>
        /// <param name="handle">A handle to the target with atleast read permissions.</param>
        /// <param name="offset">Optional offset to add to the <see cref="Pointer"/>.</param>
        public T Read(nint handle, int offset = 0) => Kernel32.ReadMemory<T>(handle, Pointer + offset);
    }
}
