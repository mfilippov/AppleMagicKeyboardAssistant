using System.Diagnostics;
using AppleMagicKeyboardAssistant.Pinvoke;

namespace AppleMagicKeyboardAssistant
{
    public class BrightnessController : IDisposable
    {
        private readonly List<PHYSICAL_MONITOR[]> _physicalMonitors = new List<PHYSICAL_MONITOR[]>();

        public BrightnessController()
        {
            var monitorHandles = new List<nint>();
            User32.EnumDisplayMonitors(nint.Zero, nint.Zero,
                (nint monitor, nint hdcMonitor, ref RECT rect, nint data) =>
                {
                    monitorHandles.Add(monitor);
                    return true;
                },
                nint.Zero);
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

        public void ChangeBrightness(int delta)
        {
            foreach (var physicalMonitor in _physicalMonitors)
            foreach (var monitor in physicalMonitor)
            {
                uint currentBrightness = 0;
                uint minBrightness = 0;
                uint maxBrightness = 0;
                Dxva2.GetMonitorBrightness(monitor.hPhysicalMonitor,
                    ref minBrightness, ref currentBrightness, ref maxBrightness);
                var newValue = currentBrightness + delta;
                if (newValue < minBrightness)
                    newValue = minBrightness;
                if (newValue > maxBrightness)
                    newValue = maxBrightness;
                Trace.WriteLine($"ChangeBrightness {currentBrightness}, {minBrightness}, {maxBrightness}, {newValue}", "AppleMagicKeyboardAssistant");
                Dxva2.SetMonitorBrightness(monitor.hPhysicalMonitor, (uint) newValue);
            }
        }
    }
}