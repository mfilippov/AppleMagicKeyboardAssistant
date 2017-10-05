using System;
using System.Collections.Generic;
using AppleMagicKeyboardAssistant.Pinvoke;
using Serilog.Core;

namespace AppleMagicKeyboardAssistant
{
    public class BrightnessController : IDisposable
    {
        private readonly Logger _logger;
        private readonly List<PHYSICAL_MONITOR[]> _physicalMonitors = new List<PHYSICAL_MONITOR[]>();

        public BrightnessController(Logger logger)
        {
            _logger = logger;
            var monitorHandles = new List<IntPtr>();
            User32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr monitor, IntPtr hdcMonitor, ref RECT rect, IntPtr data) =>
                {
                    monitorHandles.Add(monitor);
                    return true;
                },
                IntPtr.Zero);
            foreach (var monitorHandle in monitorHandles)
            {
                uint physicalMonitorsCount = 0;
                if (!Dxva2.GetNumberOfPhysicalMonitorsFromHMONITOR(monitorHandle, ref physicalMonitorsCount))
                    continue;
                var physicalMonitors = new PHYSICAL_MONITOR[physicalMonitorsCount];
                if (Dxva2.GetPhysicalMonitorsFromHMONITOR(monitorHandle, physicalMonitorsCount, physicalMonitors))
                    _physicalMonitors.Add(physicalMonitors);
            }
        }

        public void Dispose()
        {
            if (_physicalMonitors.Count <= 0)
                return;
            foreach (var t in _physicalMonitors)
            {
                var physicalMonitor = t;
                Dxva2.DestroyPhysicalMonitors((uint) t.Length, ref physicalMonitor);
            }
        }

        public void ChangeBritness(int delta)
        {
            foreach (var physicalMonitor in _physicalMonitors)
            foreach (var monitor in physicalMonitor)
            {
                uint currentBritness = 0;
                uint minBritness = 0;
                uint maxBritness = 0;
                Dxva2.GetMonitorBrightness(monitor.hPhysicalMonitor,
                    ref minBritness, ref currentBritness, ref maxBritness);
                var newValue = currentBritness + delta;
                if (newValue < minBritness)
                    newValue = minBritness;
                if (newValue > maxBritness)
                    newValue = maxBritness;
                _logger.Debug("ChangeBritness {currentBritness}, {minBritness}, {maxBritness}, {newValue}",
                    currentBritness, maxBritness, maxBritness, newValue);
                Dxva2.SetMonitorBrightness(monitor.hPhysicalMonitor, (uint) newValue);
            }
        }
    }
}