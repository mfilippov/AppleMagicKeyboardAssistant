using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using AppleMagicKeyboardAssistant.Pinvoke;
using Microsoft.Win32.SafeHandles;
using Serilog.Core;

// ReSharper disable BuiltInTypeReferenceStyle

namespace AppleMagicKeyboardAssistant
{
    public unsafe class FnKeyController : IDisposable
    {
        private readonly Logger _logger;
        private readonly FileStream _deviceFsStream;
        private readonly AppleDevice _device;
        private volatile bool _isEjectKeyPressed;

        private volatile bool _isFnKeyPressed;

        public FnKeyController(Logger logger)
        {
            _logger = logger;
            var devices = UsbDeviceManager.EnumerateAppleDevices();
            for (var i = 0; i < devices.Count - 1; i++)
            {
                devices[i].Dispose();
            }
            _device = devices[devices.Count - 1];
            _deviceFsStream = new FileStream(
                new SafeFileHandle(_device.Handle, true), FileAccess.Read, _device.BufferSize, true);
            BeginAsyncRead();
        }

        public bool IsFnKeyPressed => _isFnKeyPressed;
        public bool IsEjectKeyPressed => _isEjectKeyPressed;

        public void Dispose()
        {
            _device.Dispose();
        }

        private void BeginAsyncRead()
        {
            var arrInputReport = new byte[_device.BufferSize];
            try
            {
                _deviceFsStream.BeginRead(arrInputReport, 0, _device.BufferSize, ReadCompleted, arrInputReport);
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