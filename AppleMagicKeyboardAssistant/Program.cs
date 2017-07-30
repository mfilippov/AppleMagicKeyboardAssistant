using System.Windows.Forms;

namespace AppleMagicKeyboardAssistant
{
    internal class Program
    {
        public static void Main()
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
    }
}