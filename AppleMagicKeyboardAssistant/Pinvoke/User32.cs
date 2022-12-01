using System;
using System.Runtime.InteropServices;

// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    internal class User32
    {
        public delegate nint LowLevelKeyboardProc(Int32 nCode, nint wParam, nint lParam);

        public delegate bool MonitorEnumDelegate(nint hMonitor, nint hdcMonitor, ref RECT lprcMonitor,
            nint dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern nint SetWindowsHookEx(Int32 idHook, LowLevelKeyboardProc lpfn, nint hMod,
            UInt32 dwThreadId);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(nint hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern nint CallNextHookEx(nint hhk, Int32 nCode,
            nint wParam, nint lParam);

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs,
            [MarshalAs(UnmanagedType.LPArray)] [In] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern nint SendMessageW(nint hWnd, int msg, nint wParam, nint lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(nint hdc, nint lprcClip, MonitorEnumDelegate lpfnEnum,
            nint dwData);
    }
}