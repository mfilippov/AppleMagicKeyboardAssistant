using System;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    [Flags]
    internal enum KBDLLHOOKSTRUCTFlags : UInt32
    {
        LlkhfExtended = 0x01,
        LlkhfInjected = 0x10,
        LlkhfAltdown = 0x20,
        LlkhfUp = 0x80
    }
}