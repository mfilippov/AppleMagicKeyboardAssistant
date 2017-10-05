using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AppleMagicKeyboardAssistant.Pinvoke;
using Microsoft.Win32.SafeHandles;
using Serilog.Core;
using Constants = AppleMagicKeyboardAssistant.Pinvoke.Constants;

// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant
{
    public class FnKeyController : IDisposable
    {
        private readonly Logger _logger;
        private readonly List<UInt16> _appleVendorList = new List<UInt16>{ 0x05ac, 0x046d };
        private readonly List<UInt16> _appleProductList = new List<UInt16> { 0x23a, 0xc31c };
        private readonly FileStream _deviceFsStream;
        private readonly IntPtr _deviceHandle = IntPtr.Zero;
        private readonly short _inputBufferLength;
        private volatile bool _isEjectKeyPressed;

        private volatile bool _isFnKeyPressed;

        public FnKeyController(Logger logger)
        {
            _logger = logger;
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
                if (_appleVendorList.Contains(hidAttributes.VendorID)
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
            try
            {
                _deviceFsStream.BeginRead(arrInputReport, 0, _inputBufferLength, ReadCompleted, arrInputReport);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "ReadCompleted exception");
                Application.Exit();
            }
        }

        private void ReadCompleted(IAsyncResult ar)
        {
            var eventBuffer = ar.AsyncState as byte[];
            try
            {
                HandleKeyEvent(eventBuffer);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ReadCompleted exception");
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
            _logger.Debug("Mac special keys state changed: {_isFnKeyPressed}, {_isEjectKeyPressed}",
                _isFnKeyPressed, _isEjectKeyPressed);
        }
    }
}