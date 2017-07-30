using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
    {
        public Int32 Size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string DevicePath;
    }
}