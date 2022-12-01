using System.Diagnostics;
using System.Runtime.InteropServices;
using AppleMagicKeyboardAssistant.Pinvoke;

namespace AppleMagicKeyboardAssistant
{
    public static class UsbDeviceManager
    {
        private static readonly List<ushort> AppleVendorList = new() { 0x05ac, 0x046d };
        private static readonly List<ushort> AppleProductList = new() { 0x23a, 0x29a, 0xc31c, 0x0267, 0xc547 };
        
        public static List<AppleDevice> EnumerateAppleDevices()
        {
            var devices = new List<AppleDevice>();
            var spDeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
            spDeviceInterfaceData.CbSize = Marshal.SizeOf(spDeviceInterfaceData);
            Hid.GetHidGuid(out var hidGuid);
            var hDevInfo = SetupApi.SetupDiGetClassDevs(ref hidGuid, "", nint.Zero,
                DI_GET_CLASS_FLAGS.DIGCF_PRESENT | DI_GET_CLASS_FLAGS.DIGCF_DEVICEINTERFACE);
            for (uint i = 0;
                 SetupApi.SetupDiEnumDeviceInterfaces(hDevInfo, nint.Zero, ref hidGuid, i, ref spDeviceInterfaceData);
                 i++)
            {
                uint nRequiredSize = 0;
                var spDeviceInterfaceDetailData = new SP_DEVICE_INTERFACE_DETAIL_DATA();
                if (nint.Size == 8)
                {
                    spDeviceInterfaceDetailData.Size = 8;
                }
                else
                {
                    spDeviceInterfaceDetailData.Size = 4 + Marshal.SystemDefaultCharSize;
                }

                if (SetupApi.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref spDeviceInterfaceData,
                        nint.Zero, 0, ref nRequiredSize, nint.Zero))
                {
                    Trace.WriteLine(Marshal.GetLastPInvokeErrorMessage(), "AppleMagicKeyboardAssistant");
                    continue;
                }

                if (!SetupApi.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref spDeviceInterfaceData,
                        ref spDeviceInterfaceDetailData, nRequiredSize, ref nRequiredSize, nint.Zero))
                {
                    Trace.WriteLine(Marshal.GetLastPInvokeErrorMessage(), "AppleMagicKeyboardAssistant");
                    continue;
                }

                var handle = Kernel32.CreateFile(spDeviceInterfaceDetailData.DevicePath,
                    Constants.GENERIC_READ | Constants.GENERIC_WRITE,
                    Constants.FILE_SHARE_READ | Constants.FILE_SHARE_WRITE,
                    nint.Zero, Constants.OPEN_EXISTING, Constants.FILE_FLAG_OVERLAPPED, nint.Zero);
                var hidAttributes = new HIDD_ATTRIBUTES();
                Hid.GetAttributes(handle, ref hidAttributes);
                Trace.WriteLine($"Found device interface: [VendorID = {hidAttributes.VendorID}, ProductID = {hidAttributes.ProductID}]", "AppleMagicKeyboardAssistant");
                if (AppleVendorList.Contains(hidAttributes.VendorID)
                    && AppleProductList.Contains(hidAttributes.ProductID))
                {
                    devices.Add(new AppleDevice(handle, hidAttributes.VendorID,
                        hidAttributes.ProductID, spDeviceInterfaceDetailData.DevicePath));
                }
            }
            SetupApi.SetupDiDestroyDeviceInfoList(hDevInfo);
            return devices;
        }
    }
}