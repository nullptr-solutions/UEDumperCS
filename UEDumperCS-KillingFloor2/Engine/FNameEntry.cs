using System.Runtime.InteropServices;
using System;

namespace UEDumperCS_KillingFloor2.Engine
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FNameEntry
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x14)]
        readonly byte[]        pad_0000;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public readonly string Name;
    }
}
