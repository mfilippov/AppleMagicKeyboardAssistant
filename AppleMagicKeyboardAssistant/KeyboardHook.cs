using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AppleMagicKeyboardAssistant.Pinvoke;
using Serilog;
using Serilog.Core;

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
        private readonly IntPtr _handle = Process.GetProcessesByName("explorer").First().MainWindowHandle;
        private readonly ILogger _logger;
        private readonly IntPtr _hookId;
        private readonly IntPtr _interceptResult = (IntPtr) 1;
        private readonly IntPtr APPCOMMAND_MEDIA_NEXTTRACK = (IntPtr) 0xb0000;
        private readonly IntPtr APPCOMMAND_MEDIA_PLAY_PAUSE = (IntPtr) 0xe0000;
        private readonly IntPtr APPCOMMAND_MEDIA_PREVIOUSTRACK = (IntPtr) 0xc0000;
        private readonly IntPtr APPCOMMAND_VOLUME_DOWN = (IntPtr) 0x90000;
        private readonly IntPtr APPCOMMAND_VOLUME_MUTE = (IntPtr) 0x80000;
        private readonly IntPtr APPCOMMAND_VOLUME_UP = (IntPtr) 0xa0000;
        private readonly IntPtr WM_KEYDOWN = (IntPtr) 0x0100;
        private readonly IntPtr WM_KEYUP = (IntPtr) 0x0101;
        private readonly IntPtr WM_SYSKEYDOWN = (IntPtr) 0x0104;

        public KeyboardHook(FnKeyController fnKeyController, BrightnessController brightnessController, Logger logger)
        {
            _fnKeyController = fnKeyController;
            _brightnessController = brightnessController;
            _hookId = SetHook(Callback);
            _logger = logger;
        }

        public void Dispose()
        {
            User32.UnhookWindowsHookEx(_hookId);
        }

        private static IntPtr SetHook(User32.LowLevelKeyboardProc proc)
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

        private IntPtr Callback(int nCode, IntPtr wParam, IntPtr lParam)
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

        private bool SendCommand(IntPtr command)
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
            _brightnessController.ChangeBritness(delta);
            return true;
        }

        private bool SendKey(Keys key, KBDLLHOOKSTRUCT keyHook)
        {
            if (!_fnKeyController.IsFnKeyPressed)
                return false;
            File.AppendAllText("app.log", $"Send overritten key: {key}");
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