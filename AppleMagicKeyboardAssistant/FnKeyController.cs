using System.Diagnostics;
using Microsoft.Win32.SafeHandles;

namespace AppleMagicKeyboardAssistant
{
    public class FnKeyController : IDisposable
    {
        private readonly FileStream? _deviceFsStream;
        private readonly AppleDevice? _device;
        private volatile bool _isEjectKeyPressed;

        private volatile bool _isFnKeyPressed;
        private volatile bool _isKeyboardDeviceFound;

        public FnKeyController()
        {
            var devices = UsbDeviceManager.EnumerateAppleDevices();
            for (var i = 0; i < devices.Count - 1; i++)
            {
                devices[i].Dispose();
            }

            if (devices.Count == 0)
            {
                const string errorMessage = "Apple keyboard not found";
                Trace.WriteLine(errorMessage, "AppleMagicKeyboardAssistant");
                MessageBox.Show(errorMessage, "Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isKeyboardDeviceFound = false;
                return;
            }

            _device = devices[^1];
            _deviceFsStream = new FileStream(
                new SafeFileHandle(_device.Handle, true), FileAccess.Read, _device.BufferSize, true);
            BeginAsyncRead();
        }

        public bool IsKeyboardDeviceFound => _isKeyboardDeviceFound;

        public bool IsFnKeyPressed => _isFnKeyPressed;
        public bool IsEjectKeyPressed => _isEjectKeyPressed;

        public void Dispose()
        {
            if (_device != null)
            {
                _device.Dispose();
            }
        }

        private void BeginAsyncRead()
        {
            if (_device == null || _deviceFsStream == null)
            {
                return;
            }
            var arrInputReport = new byte[_device.BufferSize];
            try
            {
                _deviceFsStream.BeginRead(arrInputReport, 0, _device.BufferSize, ReadCompleted, arrInputReport);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ReadCompleted exception: {ex}", "AppleMagicKeyboardAssistant");
                Application.Exit();
            }
        }

        private void ReadCompleted(IAsyncResult ar)
        {
            var eventBuffer = ar.AsyncState as byte[];
            try
            {
                if (eventBuffer != null)
                {
                    HandleKeyEvent(eventBuffer);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ReadCompleted exception: {ex}", "AppleMagicKeyboardAssistant");
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
            Trace.WriteLine($"Mac special keys state changed: {_isFnKeyPressed}, {_isEjectKeyPressed}");
        }
    }
}