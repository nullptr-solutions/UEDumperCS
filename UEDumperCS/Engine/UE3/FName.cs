using System.Runtime.InteropServices;
using System;

namespace UEDumperCS.Engine.UE3
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FName
    {
        public int Index;
        public int Number;
    }
}