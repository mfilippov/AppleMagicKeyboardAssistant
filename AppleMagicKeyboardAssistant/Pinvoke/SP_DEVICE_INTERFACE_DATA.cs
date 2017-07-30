using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SP_DEVICE_INTERFACE_DATA
    {
        public Int32 CbSize;
        public Guid InterfaceClassGuid;
        public Int32 Flags;
        public Int32 Reserved;
    }
}