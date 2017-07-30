using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal struct FILE_SEGMENT_ELEMENT
    {
        [FieldOffset(0)] public IntPtr Buffer;
        [FieldOffset(0)] public UInt64 Alignment;
    }
}