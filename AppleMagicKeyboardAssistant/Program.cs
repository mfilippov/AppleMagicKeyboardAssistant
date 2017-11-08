using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AppleMagicKeyboardAssistant.Pinvoke;
using Serilog;

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
                    var result = Kernel32.ReadFile((void*)device.Handle, (void*)buffer, 0, null, ref overlapped);
                    report.AppendLine(device.ToString());
                    device.Dispose();
                }
                MessageBox.Show(report.ToString(), "Diagnostics report");
                Application.Exit();
            } 
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (basePath == null)
                return;
            var logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(basePath, "app.log"), fileSizeLimitBytes: 10240)
                .CreateLogger();
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += (sender, eventArgs) =>
                {
                    logger.Error(eventArgs.Exception, "");
                };
                var contextMenu = new ContextMenu();
                contextMenu.MenuItems.Add("E&xit", (sender, eventArgs) => Application.Exit());
                using (var ni = new NotifyIcon())
                {
                    ni.Icon = new Icon(typeof(Program), "icon.ico");
                    ni.ContextMenu = contextMenu;
                    ni.Visible = true;
                    using (var brightnessController = new BrightnessController(logger))
                    {
                        using (var fnKeyController = new FnKeyController(logger))
                        {
                            using (new KeyboardHook(fnKeyController, brightnessController, logger))
                            {
                                try
                                {
                                    logger.Information("Application started");
                                    Application.Run();
                                    logger.Information("Application exiting");
                                }
                                catch (Exception ex)
                                {
                                    logger.Fatal(ex, "Unhandled error");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Unhandled error");
            }
        }
    }
}