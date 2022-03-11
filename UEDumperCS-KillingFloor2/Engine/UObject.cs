using UEDumperCS.Engine.UE3;
using UEDumperCS.Interop;
using UEDumperCS.Utils;

using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;

namespace UEDumperCS_KillingFloor2.Engine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UObject
    {
        public readonly nint            VTablePointer;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x30)]
        readonly byte[]                 pad_0008;
                              
        public readonly int             ObjectInternalInteger;
        public readonly int             NetIndex;
        public readonly Remote<UObject> Outer;
        public readonly FName           Name;
        public readonly Remote<UObject> Inner;
        public readonly Remote<UObject> ObjectArchetype;

        /// <summary>
        /// Retrieves a name from the names table by its index.
        /// </summary>
        /// <param name="names">The name table dump.</param>
        /// <param name="clean">If true returns the string after the last '/'.</param>
        public string GetName(ref Dictionary<int, FNameEntry> names, bool clean = true)
        {
            if (!names.TryGetValue(Name.Index, out var entry))
                return null;

            var name = Name.Number > 0
                ? entry.Name + $"_{Name.Number}"
                : entry.Name;

            if (clean)
            {
                var slashIdx = name.LastIndexOf('/');
                if (slashIdx is not -1)
                    return name[(slashIdx + 1)..];
            }

            return name;
        }

        /// <summary>
        /// Retrieves the full name for the current <see cref="UObject"/>.
        /// </summary>
        /// <param name="handle">A handle to the target with atleast read permissions.</param>
        /// <param name="names">The name table dump.</param>
        /// <param name="clean">If true returns the string after the last '/'.</param>
        public string GetFullName(nint handle, ref Dictionary<int, FNameEntry> names, bool clean = true)
        {
            var name = GetName(ref names, clean);
            if (name is null)
                return null;

            if (Outer.IsValid)
            {
                for (var outerObj = Outer.Read(handle); outerObj.VTablePointer is not 0; outerObj = outerObj.Outer.Read(handle))
                {
                    var outerName = outerObj.GetName(ref names, clean);
                    if (outerName is null)
                        break;

                    name = outerName + '.' + name;

                    if (!outerObj.Outer.IsValid)
                        break;
                }
            }

            return name;
        }
    }
}