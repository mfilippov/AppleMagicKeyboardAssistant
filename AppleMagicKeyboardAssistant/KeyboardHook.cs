using System.Diagnostics;
using System.Runtime.InteropServices;
using AppleMagicKeyboardAssistant.Pinvoke;

// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable InconsistentNaming

namespace AppleMagicKeyboardAssistant
{
    public class KeyboardHook : IDisposable
    {
        private const int WhKeyboardLl = 13;

        private const int WM_APPCOMMAND = 0x319;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private readonly BrightnessController _brightnessController;
        private readonly UIntPtr _extraInfo = (UIntPtr) 0x37564;
        private readonly FnKeyController _fnKeyController;
        private readonly nint _handle = Process.GetProcessesByName("explorer").First().MainWindowHandle;
        private readonly nint _hookId;
        private readonly nint _interceptResult = (nint) 1;
        private readonly nint APPCOMMAND_MEDIA_NEXTTRACK = (nint) 0xb0000;
        private readonly nint APPCOMMAND_MEDIA_PLAY_PAUSE = (nint) 0xe0000;
        private readonly nint APPCOMMAND_MEDIA_PREVIOUSTRACK = (nint) 0xc0000;
        private readonly nint APPCOMMAND_VOLUME_DOWN = (nint) 0x90000;
        private readonly nint APPCOMMAND_VOLUME_MUTE = (nint) 0x80000;
        private readonly nint APPCOMMAND_VOLUME_UP = (nint) 0xa0000;
        private readonly nint WM_KEYDOWN = (nint) 0x0100;
        private readonly nint WM_KEYUP = (nint) 0x0101;
        private readonly nint WM_SYSKEYDOWN = (nint) 0x0104;

        public KeyboardHook(FnKeyController fnKeyController, BrightnessController brightnessController)
        {
            _fnKeyController = fnKeyController;
            _brightnessController = brightnessController;
            _hookId = SetHook(Callback);
        }

        public void Dispose()
        {
            User32.UnhookWindowsHookEx(_hookId);
        }

        private static nint SetHook(User32.LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            {
                using (var curModule = curProcess.MainModule)
                {
                    return User32.SetWindowsHookEx(WhKeyboardLl, proc, Kernel32.GetModuleHandle(curModule.ModuleName),
                        0);
                }
            }
        }

        private nint Callback(int nCode, nint wParam, nint lParam)
        {
            if (nCode < 0
                || wParam != WM_KEYDOWN
                && wParam != WM_SYSKEYDOWN
                && wParam != WM_KEYUP)
                return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);

            var kbd = (KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
            if (nCode != 0)
                return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
            if ((kbd.flags & KBDLLHOOKSTRUCTFlags.LlkhfInjected) == KBDLLHOOKSTRUCTFlags.LlkhfInjected)
                return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
            var intercept = HandleKeyPressedEvent(kbd);
            return intercept ? _interceptResult : User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private bool HandleKeyPressedEvent(KBDLLHOOKSTRUCT keyHook)
        {
            var key = (Keys) keyHook.vkCode;

            if (keyHook.dwExtraInfo == _extraInfo)
                return false;
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (key)
            {
                case Keys.LControlKey:
                case Keys.RControlKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                case Keys.LMenu:
                case Keys.RMenu:
                    return false;
                case Keys.Left:
                    return SendKey(Keys.Home, keyHook);
                case Keys.Right:
                    return SendKey(Keys.End, keyHook);
                case Keys.Up:
                    return SendKey(Keys.PageUp, keyHook);
                case Keys.Down:
                    return SendKey(Keys.PageDown, keyHook);
                case Keys.Back:
                    return (keyHook.flags & KBDLLHOOKSTRUCTFlags.LlkhfUp) != KBDLLHOOKSTRUCTFlags.LlkhfUp && SendKey(Keys.Delete, keyHook);
                case Keys.Enter:
                    return (keyHook.flags & KBDLLHOOKSTRUCTFlags.LlkhfUp) != KBDLLHOOKSTRUCTFlags.LlkhfUp && SendKey(Keys.Insert, keyHook);
                case Keys.F1:
                    return (keyHook.flags & KBDLLHOOKSTRUCTFlags.LlkhfUp) != KBDLLHOOKSTRUCTFlags.LlkhfUp && Bright(-10);
                case Keys.F2:
                    return (keyHook.flags & KBDLLHOOKSTRUCTFlags.LlkhfUp) != KBDLLHOOKSTRUCTFlags.LlkhfUp && Bright(10);
                case Keys.F7:
                    return (keyHook.flags & KBDLLHOOKSTRUCTFlags.LlkhfUp) != KBDLLHOOKSTRUCTFlags.LlkhfUp && SendCommand(APPCOMMAND_MEDIA_PREVIOUSTRACK);
                case Keys.F8:
                    return (keyHook.flags & KBDLLHOOKSTRUCTFlags.LlkhfUp) != KBDLLHOOKSTRUCTFlags.LlkhfUp && SendCommand(APPCOMMAND_MEDIA_PLAY_PAUSE);
                case Keys.F9:
                    return (keyHook.flags & KBDLLHOOKSTRUCTFlags.LlkhfUp) != KBDLLHOOKSTRUCTFlags.LlkhfUp && SendCommand(APPCOMMAND_MEDIA_NEXTTRACK);
                case Keys.F10:
                    return (keyHook.flags & KBDLLHOOKSTRUCTFlags.LlkhfUp) != KBDLLHOOKSTRUCTFlags.LlkhfUp && SendCommand(APPCOMMAND_VOLUME_MUTE);
                case Keys.F11:
                    return (keyHook.flags & KBDLLHOOKSTRUCTFlags.LlkhfUp) != KBDLLHOOKSTRUCTFlags.LlkhfUp && SendCommand(APPCOMMAND_VOLUME_DOWN);
                case Keys.F12:
                    return (keyHook.flags & KBDLLHOOKSTRUCTFlags.LlkhfUp) != KBDLLHOOKSTRUCTFlags.LlkhfUp && SendCommand(APPCOMMAND_VOLUME_UP);
                default:
                    return false;
            }
        }

        private bool SendCommand(nint command)
        {
            if (!_fnKeyController.IsFnKeyPressed)
                return false;
            User32.SendMessageW(_handle, WM_APPCOMMAND, _handle, command);
            return true;
        }

        private bool Bright(int delta)
        {
            if (!_fnKeyController.IsFnKeyPressed)
                return false;
            _brightnessController.ChangeBrightness(delta);
            return true;
        }

        private bool SendKey(Keys key, KBDLLHOOKSTRUCT keyHook)
        {
            if (!_fnKeyController.IsFnKeyPressed)
                return false;
            Trace.WriteLine($"Send overwritten key: {key}, {keyHook}", "AppleMagicKeyboardAssistant");
            var inputs = new INPUT[1];
            inputs[0] = new INPUT
            {
                Type = INPUT_TYPE.INPUT_KEYBOARD,
                Data =
                {
                    Keyboard =
                        new KEYBDINPUT
                        {
                            KeyCode = (UInt16) key,
                            ExtraInfo = keyHook.dwExtraInfo,
                            Flags = (uint)keyHook.flags,
                            Time = keyHook.time
                        }
                }
            };

            User32.SendInput((uint) inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            return true;
        }
    }
}