using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AppleMagicKeyboardAssistant.Pinvoke;

namespace AppleMagicKeyboardAssistant
{
    public static class UsbDeviceManager
    {
        private static readonly List<ushort> AppleVendorList = new List<ushort>{ 0x05ac, 0x046d };
        private static readonly List<ushort> AppleProductList = new List<ushort> { 0x23a, 0xc31c, 0x0267 };
        
        public static List<AppleDevice> EnumerateAppleDevices()
        {
            var devices = new List<AppleDevice>();
            var spDeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
            spDeviceInterfaceData.CbSize = Marshal.SizeOf(spDeviceInterfaceData);
            Hid.HidD_GetHidGuid(out var hidGuid);
            var hDevInfo = SetupApi.SetupDiGetClassDevs(ref hidGuid, null, IntPtr.Zero,
                DI_GET_CLASS_FLAGS.DIGCF_DEVICEINTERFACE);
            SetupApi.SetupDiEnumDeviceInterfaces(hDevInfo, 0, ref hidGuid, 0,
                ref spDeviceInterfaceData);
            for (uint i = 0;
                SetupApi.SetupDiEnumDeviceInterfaces(hDevInfo, 0, ref hidGuid, i, ref spDeviceInterfaceData);
                i++)
            {
                uint nRequiredSize = 0;
                var spDeviceInterfaceDetailData = new SP_DEVICE_INTERFACE_DETAIL_DATA {Size = 5};

                if (SetupApi.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref spDeviceInterfaceData,
                    IntPtr.Zero, nRequiredSize, ref nRequiredSize, IntPtr.Zero))
                    continue;
                if (!SetupApi.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref spDeviceInterfaceData,
                    ref spDeviceInterfaceDetailData, nRequiredSize, ref nRequiredSize, IntPtr.Zero))
                    continue;
                var handle = Kernel32.CreateFile(spDeviceInterfaceDetailData.DevicePath,
                    Constants.GENERIC_READ | Constants.GENERIC_WRITE,
                    Constants.FILE_SHARE_READ | Constants.FILE_SHARE_WRITE,
                    IntPtr.Zero, Constants.OPEN_EXISTING, Constants.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
                var hidAttributes = new HIDD_ATTRIBUTES();
                Hid.HidD_GetAttributes(handle, ref hidAttributes);
                if (AppleVendorList.Contains(hidAttributes.VendorID)
                    && AppleProductList.Contains(hidAttributes.ProductID))
                {
                    devices.Add(new AppleDevice(handle, hidAttributes.VendorID,
                        hidAttributes.ProductID, spDeviceInterfaceDetailData.DevicePath));
                }
            }
            return devices;
        }
    }
}