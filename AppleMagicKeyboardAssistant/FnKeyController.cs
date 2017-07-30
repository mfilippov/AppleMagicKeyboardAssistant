﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using AppleMagicKeyboardAssistant.Pinvoke;
using Microsoft.Win32.SafeHandles;

namespace AppleMagicKeyboardAssistant
{
    public class FnKeyController : IDisposable
    {
        private readonly List<short> _appleProductList = new List<short> {570};
        private readonly FileStream _deviceFsStream;
        private readonly IntPtr _deviceHandle = IntPtr.Zero;
        private readonly short _inputBufferLength;
        private volatile bool _isEjectKeyPressed;

        private volatile bool _isFnKeyPressed;

        public FnKeyController()
        {
            Guid hidGuid;
            var spDeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
            spDeviceInterfaceData.CbSize = Marshal.SizeOf(spDeviceInterfaceData);
            Hid.HidD_GetHidGuid(out hidGuid);
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
                if (hidAttributes.VendorID == Constants.VID_APPLE
                    && _appleProductList.Contains(hidAttributes.ProductID))
                {
                    if (_deviceHandle != null)
                        Kernel32.CloseHandle(_deviceHandle);
                    _deviceHandle = handle;
                }
                if (handle != _deviceHandle)
                    Kernel32.CloseHandle(handle);
            }
            if (_deviceHandle == IntPtr.Zero)
                return;
            var pData = IntPtr.Zero;
            try
            {
                if (!Hid.HidD_GetPreparsedData(_deviceHandle, out pData))
                    return;

                HIDP_CAPS hidpCaps;
                Hid.HidP_GetCaps(pData, out hidpCaps);
                _inputBufferLength = hidpCaps.InputReportByteLength;
                _deviceFsStream = new FileStream(new SafeFileHandle(_deviceHandle, true), FileAccess.Read,
                    _inputBufferLength, true);
                BeginAsyncRead();
            }
            finally
            {
                Hid.HidD_FreePreparsedData(ref pData);
            }
        }

        public bool IsFnKeyPressed => _isFnKeyPressed;
        public bool IsEjectKeyPressed => _isEjectKeyPressed;

        public void Dispose()
        {
            if (_deviceHandle != IntPtr.Zero)
                Kernel32.CloseHandle(_deviceHandle);
        }

        private void BeginAsyncRead()
        {
            var arrInputReport = new byte[_inputBufferLength];
            _deviceFsStream.BeginRead(arrInputReport, 0, _inputBufferLength, ReadCompleted, arrInputReport);
        }

        private void ReadCompleted(IAsyncResult ar)
        {
            var eventBuffer = ar.AsyncState as byte[];
            try
            {
                HandleKeyEvent(eventBuffer);
            }
            finally
            {
                BeginAsyncRead();
            }
        }

        private void HandleKeyEvent(IReadOnlyList<byte> eventBuffer)
        {
            if (eventBuffer[0] != 17)
                return;
            _isFnKeyPressed = (eventBuffer[1] & 16) == 16;
            _isEjectKeyPressed = (eventBuffer[1] & 8) == 8;
        }
    }
}