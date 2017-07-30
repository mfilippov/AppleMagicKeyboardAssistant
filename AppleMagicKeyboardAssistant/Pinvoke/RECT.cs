using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}