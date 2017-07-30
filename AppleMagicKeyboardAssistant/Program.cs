using System;
using System.Drawing;
using System.Windows.Forms;

namespace AppleMagicKeyboardAssistant
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("E&xit", (sender, args) => Application.Exit());
            using (var ni = new NotifyIcon())
            {
                ni.Icon = new Icon(typeof(Program), "icon.ico");
                ni.ContextMenu = contextMenu;
                ni.Visible = true;
                using (var brightnessController = new BrightnessController())
                {
                    using (var fnKeyController = new FnKeyController())
                    {
                        using (new KeyboardHook(fnKeyController, brightnessController))
                        {
                            Application.Run();
                        }
                    }
                }
            }
        }
    }
}