using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class KBDLLHOOKSTRUCT
    {
        public UIntPtr dwExtraInfo;
        public KBDLLHOOKSTRUCTFlags flags;
        public UInt32 scanCode;
        public UInt32 time;
        public UInt32 vkCode;
    }
}