using System;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    internal static class Constants
    {
        public const UInt32 GENERIC_READ = 0x80000000;
        public const UInt32 GENERIC_WRITE = 0x40000000;
        public const UInt32 FILE_SHARE_READ = 0x00000001;
        public const UInt32 FILE_SHARE_WRITE = 0x00000002;
        public const UInt32 OPEN_EXISTING = 3;
        public const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;
        public const Int16 VID_APPLE = 0x05ac;
    }
}