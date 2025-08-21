using System;
using System.Windows.Forms;

namespace Shorthand.Windows
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Console.WriteLine("Shorthand for Windows");
            
            // Create and show the main form (which will be hidden but manages the tray icon)
            var mainForm = new MainForm();
            Application.Run(mainForm);
        }
    }
}
