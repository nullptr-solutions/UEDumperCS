using System.Runtime.InteropServices;
using System;

namespace UEDumperCS.Engine.UE4
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FName
    {
        public int Index;
        public int Number;
    }
}