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

        [STAThread]
        static void Main(string[] args)
        {
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            if (args.Length > 0)
            {
                module = args[0];
            }
            if (args.Length > 0)
            {
                siteNumber = Convert.ToInt32(args[1]);
            }
            if (args.Length > 1)
            {
                duts = args[2];
            }
            if (args.Length > 2)
            {
                profilePath = args[3];
            } 

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FinalTestForm());
        }

    }
}
