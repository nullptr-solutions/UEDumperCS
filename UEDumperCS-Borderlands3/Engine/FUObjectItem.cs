using UEDumperCS.Utils;

using System.Runtime.InteropServices;
using System;

namespace UEDumperCS_Borderlands3.Engine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FUObjectItem
    {
        public readonly Remote<UObject> Object;
        public readonly int             Flags;
        public readonly int             ClusterRootIndex;
        public readonly int             SerialNumber;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x4)]
        readonly byte[]                 pad_0014;
    }
}
