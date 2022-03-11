using System.Runtime.InteropServices;
using System;

namespace UEDumperCS_RocketLeague.Engine
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FNameEntry
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x18)]
        readonly byte[]        pad_0000;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public readonly string Name;
    }
}