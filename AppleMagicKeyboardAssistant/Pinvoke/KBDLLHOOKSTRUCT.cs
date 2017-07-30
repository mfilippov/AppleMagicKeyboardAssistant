using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class KBDLLHOOKSTRUCT
    {
        public UInt32 vkCode;
        public UInt32 scanCode;
        public KBDLLHOOKSTRUCTFlags flags;
        public UInt32 time;
        public UIntPtr dwExtraInfo;
        
    }
}