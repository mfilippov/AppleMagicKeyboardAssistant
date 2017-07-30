using System;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    internal struct KEYBDINPUT
    {
        public UInt16 KeyCode;
        public UInt16 Scan;
        public UInt32 Flags;
        public UInt32 Time;
        public UIntPtr ExtraInfo;
    }
}