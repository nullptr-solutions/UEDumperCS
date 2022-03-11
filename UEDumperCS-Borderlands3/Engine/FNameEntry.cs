using System.Runtime.InteropServices;
using System;

namespace UEDumperCS_Borderlands3.Engine
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FNameEntry
    {
        public readonly int    Index;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x4)]
        readonly byte[]        pad_0004;
        public readonly nint   HashNext;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public readonly string Name;
    }
}
