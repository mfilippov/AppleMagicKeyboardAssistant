using System.ServiceProcess;
using System.Threading;

namespace AppleMagicKeyboardAssistant
{
    internal class Program
    {
        public static void Main()
        {
            ServiceBase.Run(new AppleMagicKeyboardAssitantService());
        }
    }

    public class AppleMagicKeyboardAssitantService : ServiceBase
    {
        private Thread _serviceThread;
        private ManualResetEvent _waiter;
        
        public AppleMagicKeyboardAssitantService()
        {
            ServiceName = "AppleMagicKeyboardAssitant";
            CanStop = true;
            CanPauseAndContinue = false;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            _waiter = new ManualResetEvent(false); 
            _serviceThread = new Thread(() =>
            {
                using (var brightnessController = new BrightnessController())
                {
                    using (var fnKeyController = new FnKeyController())
                    {
                        using (new KeyboardHook(fnKeyController, brightnessController))
                        {
                            _waiter.WaitOne();
                        }
                    }
                }
            });
            _serviceThread.Start();
        }

        protected override void OnStop()
        {
            _waiter.Set();
        }
    }
}