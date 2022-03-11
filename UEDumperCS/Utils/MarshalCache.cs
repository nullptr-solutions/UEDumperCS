using System.Runtime.InteropServices;
using System;

namespace UEDumperCS.Utils
{
    public static class MarshalCache<T>
    {
        /// <summary>
        /// Initializes the static <see cref="MarshalCache{T}"/>.
        /// </summary>
        static MarshalCache() => Size = Marshal.SizeOf(default(T));

        /// <summary>
        /// The size in bytes of <see cref="T"/>.
        /// </summary>
        public static readonly int Size;
    }
}