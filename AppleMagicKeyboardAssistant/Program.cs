using System.ServiceProcess;
using System.Windows.Forms;

namespace AppleMagicKeyboardAssistant
{
    internal class Program
    {
        public static void Main()
        {
            ServiceBase.Run(new AppleMagicKeyboardAssitant());
        }
    }

    public class AppleMagicKeyboardAssitant : ServiceBase
    {
        public AppleMagicKeyboardAssitant()
        {
            ServiceName = "AppleMagicKeyboardAssitant";
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
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

        protected override void OnStop()
        {
            Application.Exit();
        }
    }
}