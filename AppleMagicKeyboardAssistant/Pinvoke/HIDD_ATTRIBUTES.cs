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
        public Int16 VendorID;
        public Int16 ProductID;
        public Int16 VersionNumber;
    }
}