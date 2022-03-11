using UEDumperCS.Utils;

using System.Runtime.InteropServices;
using System.Linq;

namespace UEDumperCS.Interop
{
    public static class Kernel32
    {
        #region Imports

        /// <summary>
        /// Reads memory from <paramref name="baseAddress"/> with <paramref name="handle"/> and
        /// <paramref name="size"/> into <paramref name="buffer"/>.
        /// </summary>
        /// <param name="handle">The handle for reading memory.</param>
        /// <param name="baseAddress">The address to read memory from.</param>
        /// <param name="buffer">The buffer for the memory that gets read.</param>
        /// <param name="size">The size of the memory to read.</param>
        /// <param name="numberOfBytesRead">The number of bytes that got read.</param>
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(
            nint         handle,
            nint         baseAddress,
            [Out] byte[] buffer,
            int          size,
            out nint     numberOfBytesRead);

        #endregion

        #region Wrapper Methods

        /// <summary>
        /// Converts <paramref name="bytes"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert the bytes to.</typeparam>
        /// <param name="bytes">The bytes.</param>
        public static T ByteArrayToGeneric<T>(ref byte[] bytes)
        {
            GCHandle handle = default;

            try
            {
                handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }

        /// <summary>
        /// Reads memory from <paramref name="address"/> with <paramref name="handle"/> and <paramref name="size"/>
        /// </summary>
        /// <param name="handle">The handle for reading memory.</param>
        /// <param name="address">The address to read memory from.</param>
        /// <param name="size">The size of the memory to read.</param>
        public static byte[] ReadMemory(nint handle, nint address, int size)
        {
            var buf = new byte[size];
            if (!ReadProcessMemory(handle, address, buf, buf.Length, out _))
                return null;

            return buf;
        }

        /// <summary>
        /// Reads memory from <paramref name="address"/> with <paramref name="handle"/> and casts the bytes to
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert the bytes to.</typeparam>
        /// <param name="handle">The handle for reading memory.</param>
        /// <param name="address">The address to read memory from.</param>
        public static T ReadMemory<T>(nint handle, nint address)
        {
            var size = MarshalCache<T>.Size;

            var buf = ReadMemory(handle, address, size);
            if (buf?.Any() == true)
                return ByteArrayToGeneric<T>(ref buf);

            return default;
        }

        #endregion
    }
}