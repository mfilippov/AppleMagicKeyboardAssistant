using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Serilog;

namespace AppleMagicKeyboardAssistant
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (basePath == null)
                    return;
                var logger = new LoggerConfiguration()
                    .WriteTo.File(Path.Combine(basePath, "app.log"), fileSizeLimitBytes: 10240)
                    .CreateLogger();
                logger.Information("Application started");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += (sender, args) =>
                {
                    logger.Error(args.Exception, "");
                };
                var contextMenu = new ContextMenu();
                contextMenu.MenuItems.Add("E&xit", (sender, args) => Application.Exit());
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
                                    Application.Run();
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
                File.AppendAllText("app.log", ex.Message);
                File.AppendAllText("app.log", ex.StackTrace);
            }
        }
    }
}