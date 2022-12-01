using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    internal class Hid
    {
        [DllImport("hid.dll", EntryPoint = "HidD_GetHidGuid", SetLastError = true)]
        public static extern void GetHidGuid(out Guid guid);

        [DllImport("hid.dll", EntryPoint = "HidD_GetAttributes",SetLastError = true)]
        public static extern bool GetAttributes(nint deviceObject, ref HIDD_ATTRIBUTES attributes);

        [DllImport("hid.dll", EntryPoint = "HidD_GetPreparsedData", SetLastError = true)]
        public static extern bool GetPreparsedData(nint hFile, out nint lpData);

        [DllImport("hid.dll", EntryPoint = "HidP_GetCaps", SetLastError = true)]
        public static extern int GetCaps(nint lpData, out HIDP_CAPS oCaps);

        [DllImport("hid.dll", EntryPoint = "HidD_FreePreparsedData", SetLastError = true)]
        public static extern bool FreePreparsedData(ref nint pData);

        [DllImport("hid.dll", EntryPoint = "HidD_GetProductString", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GetProductString(nint deviceObject, StringBuilder buffer, Int32 bufferLength);
    }
}