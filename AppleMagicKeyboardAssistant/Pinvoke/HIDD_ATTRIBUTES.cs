using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct HIDD_ATTRIBUTES
    {
        public Int32 Size;
        public UInt16 VendorID;
        public UInt16 ProductID;
        public UInt16 VersionNumber;
    }
}