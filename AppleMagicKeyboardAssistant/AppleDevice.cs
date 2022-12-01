using System;
using System.Text;
using AppleMagicKeyboardAssistant.Pinvoke;

namespace AppleMagicKeyboardAssistant
{
    public class AppleDevice: IDisposable
    {
        public nint Handle { get; }
        public ushort VendorId { get; }
        public ushort ProcuctId { get; }
        public string DevicePath { get; }
        public string ProductString { get; }
        public int BufferSize { get; }

        public AppleDevice(nint handle, ushort vendorId, ushort procuctId, string devicePath)
        {
            Handle = handle;
            VendorId = vendorId;
            ProcuctId = procuctId;
            DevicePath = devicePath;
            var sb = new StringBuilder(1024);
            Hid.GetProductString(handle, sb, 1024);
            ProductString = sb.ToString();
            var pData = nint.Zero;
            try
            {
                if (!Hid.GetPreparsedData(Handle, out pData))
                {
                    throw new Exception("Invalid handle");
                }
                Hid.GetCaps(pData, out var hidpCaps);
                BufferSize = hidpCaps.InputReportByteLength;
            }
            finally
            {
                Hid.FreePreparsedData(ref pData);
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