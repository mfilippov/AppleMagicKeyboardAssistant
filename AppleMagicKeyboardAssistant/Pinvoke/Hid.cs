using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    internal class Hid
    {
        [DllImport("hid.dll", SetLastError = true)]
        public static extern void HidD_GetHidGuid(out Guid guid);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetAttributes(IntPtr deviceObject, ref HIDD_ATTRIBUTES attributes);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetPreparsedData(IntPtr hFile, out IntPtr lpData);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern int HidP_GetCaps(IntPtr lpData, out HIDP_CAPS oCaps);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_FreePreparsedData(ref IntPtr pData);

        [DllImport("hid.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool HidD_GetProductString(IntPtr deviceObject, StringBuilder buffer, Int32 bufferLength);
    }
}