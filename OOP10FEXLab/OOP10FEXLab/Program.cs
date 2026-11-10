using System;
using System.Windows.Forms;
using OOP10FEXLab.Core;
using OOP10FEXLab.Kiosk;
using OOP10FEXLab.QueueMonitor;

namespace OOP10FEXLab
{
    internal static class Program
    {
        /// <summary>
        /// Start options:
        ///   OOP10FEXLab.exe           -> launcher (open Kiosk, Queue Monitor, or both)
        ///   OOP10FEXLab.exe kiosk     -> Kiosk only
        ///   OOP10FEXLab.exe monitor   -> Queue Monitor only
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppServices.Initialize();

            string mode = args.Length > 0 ? args[0].ToLowerInvariant() : "";
            if (mode == "kiosk") Application.Run(new KioskForm());
            else if (mode == "monitor") Application.Run(new QueueMonitorForm());
            else Application.Run(new LauncherForm());
        }
    }
}
