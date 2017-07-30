using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant.Pinvoke
{
    internal class SetupApi
    {
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid,
            [MarshalAs(UnmanagedType.LPTStr)] string enumerator, IntPtr hwndParent, DI_GET_CLASS_FLAGS flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, UInt32 nDeviceInfoData,
            ref Guid interfaceClassGuid, UInt32 memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo,
            ref SP_DEVICE_INTERFACE_DATA spDeviceInterfaceData,
            IntPtr spDeviceInterfaceDetailData, UInt32 deviceInterfaceDetailDataSize,
            ref UInt32 requiredSize, IntPtr deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo,
            ref SP_DEVICE_INTERFACE_DATA spDeviceInterfaceData,
            ref SP_DEVICE_INTERFACE_DETAIL_DATA spDeviceInterfaceDetailData, UInt32 deviceInterfaceDetailDataSize,
            ref UInt32 requiredSize, IntPtr deviceInfoData);
    }
}