using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    internal unsafe class Kernel32
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern nint GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern nint CreateFile([MarshalAs(UnmanagedType.LPStr)] string strName, UInt32 access,
            UInt32 shareMode, nint security, UInt32 creationFlags, UInt32 attributes, nint template);

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool CloseHandle(nint hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true, ExactSpelling = true)]
        public static extern Int32 ReadFile(void* hFile, void* lpBuffer, UInt32 nNumberOfBytesToRead, UInt32* lpNumberOfBytesRead, ref NativeOverlapped lpOverlapped);
    }
}