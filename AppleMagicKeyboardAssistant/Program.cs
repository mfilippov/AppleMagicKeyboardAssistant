using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using AppleMagicKeyboardAssistant.Pinvoke;

namespace AppleMagicKeyboardAssistant
{
    public unsafe class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "diag")
            {
                var report = new StringBuilder("USB Devices:\n");
                foreach (var device in UsbDeviceManager.EnumerateAppleDevices())
                {
                    var overlapped = default(NativeOverlapped);
                    var buffer = Marshal.AllocHGlobal(device.BufferSize);
                    var result = Kernel32.ReadFile((void*)device.Handle, (void*)buffer, (uint)device.BufferSize, null, ref overlapped);
                    report.AppendLine($"{device}\n\tRead result: {result}");
                    device.Dispose();
                }
                MessageBox.Show(report.ToString(), "Diagnostics report");
                Application.Exit();
                return;
            } 
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += (sender, eventArgs) =>
                {
                    Trace.WriteLine(eventArgs.Exception, "AppleMagicKeyboardAssistant");
                };
                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("E&xit",
                    Image.FromStream(typeof(Program).Assembly.GetManifestResourceStream("AppleMagicKeyboardAssistant.Exit.png")
                                     ?? throw new InvalidOperationException("Can't load resource 'Exit.png'")),
                    (_, _) => Application.Exit());
                using (var ni = new NotifyIcon())
                {
                    ni.Icon = new Icon(typeof(Program), "App.ico");
                    ni.ContextMenuStrip = contextMenu;
                    ni.Visible = true;
                    using (var brightnessController = new BrightnessController())
                    {
                        using (var fnKeyController = new FnKeyController())
                        {
                            if (!fnKeyController.IsKeyboardDeviceFound)
                            {
                                return;
                            }
                            using (new KeyboardHook(fnKeyController, brightnessController))
                            {
                                try
                                {
                                    Trace.WriteLine("Application started", "AppleMagicKeyboardAssistant");
                                    Application.Run();
                                    Trace.WriteLine("Application exiting", "AppleMagicKeyboardAssistant");
                                }
                                catch (Exception ex)
                                {
                                    Trace.WriteLine($"Unhandled error: {ex}", "AppleMagicKeyboardAssistant");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Unhandled error: {ex}", "AppleMagicKeyboardAssistant");
            }
        }
    }
}