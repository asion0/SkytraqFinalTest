using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Globalization;

namespace FinalTestV8
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        /// 
        public static ResourceManager rm = new ResourceManager("FinalTestV8.LanguagePack", Assembly.GetExecutingAssembly());
        public static string profilePath;
        public static int siteNumber;
        public static string duts;
        public static string module;
        public static string workingNumber;

        [STAThread]
        static void Main(string[] args)
        {
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            if (args.Length > 0)
            {
                module = args[0];
            }
            if (args.Length > 1)
            {
                siteNumber = Convert.ToInt32(args[1]);
            }
            if (args.Length > 2)
            {
                duts = args[2];
            }
            if (args.Length > 3)
            {
                profilePath = args[3];
            }
            if (args.Length > 4)
            {
                workingNumber = args[4];
            }
            else
            {
                workingNumber = "A000-00000000000";
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FinalTestForm());
        }

    }
}
