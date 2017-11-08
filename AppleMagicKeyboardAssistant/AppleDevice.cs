using System;
using System.Text;
using AppleMagicKeyboardAssistant.Pinvoke;

namespace AppleMagicKeyboardAssistant
{
    public class AppleDevice: IDisposable
    {
        public IntPtr Handle { get; }
        public ushort VendorId { get; }
        public ushort ProcuctId { get; }
        public string DevicePath { get; }
        public string ProductString { get; }
        public int BufferSize { get; }

        public AppleDevice(IntPtr handle, ushort vendorId, ushort procuctId, string devicePath)
        {
            Handle = handle;
            VendorId = vendorId;
            ProcuctId = procuctId;
            DevicePath = devicePath;
            var sb = new StringBuilder(1024);
            Hid.HidD_GetProductString(handle, sb, 1024);
            ProductString = sb.ToString();
            var pData = IntPtr.Zero;
            try
            {
                if (!Hid.HidD_GetPreparsedData(Handle, out pData))
                {
                    throw new Exception("Invalid handle");
                }
                Hid.HidP_GetCaps(pData, out var hidpCaps);
                BufferSize = hidpCaps.InputReportByteLength;
            }
            finally
            {
                Hid.HidD_FreePreparsedData(ref pData);
            }
        }

        public void Dispose()
        {
            Kernel32.CloseHandle(Handle);
        }

        public override string ToString()
        {
            return $"{nameof(Handle)}: {Handle}, {nameof(VendorId)}: {VendorId}, " +
                   $"{nameof(ProcuctId)}: {ProcuctId}, {nameof(DevicePath)}: {DevicePath}, " +
                   $"{nameof(ProductString)}: {ProductString}, {nameof(BufferSize)}: {BufferSize}";
        }
    }
}