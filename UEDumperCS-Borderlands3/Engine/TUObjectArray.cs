using System.Runtime.InteropServices;
using System;

namespace UEDumperCS_Borderlands3.Engine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TUObjectArray
    {
        public readonly int                                     ObjFirstGCIndex;
        public readonly int                                     ObjLastNonGCIndex;
        public readonly int                                     MaxObjectsNotConsideredByGC;
        public readonly int                                     OpenForDisregardForGC;
        public readonly FChunkedFixedUObjectArray<FUObjectItem> ObjObjects;
    }
}
