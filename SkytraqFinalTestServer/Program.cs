using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SkytraqFinalTestServer
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ServerForm());
        }

        public class ProgramVersion
        {
            public ProgramVersion()
            {
#if _V815_
                testingType = TestingType.V815;
#elif _V816_
                testingType = TestingType.V816;
#elif _V822_
                testingType = TestingType.V822;
#else
                testingType = TestingType.Generic;
#endif
            }

            public enum TestingType
            {
                Generic,
                V815,
                V816,
                V822
            }
            public TestingType testingType = TestingType.Generic;
            public bool IsV815() { return testingType == TestingType.V815; }
            public bool IsV816() { return testingType == TestingType.V816; }
            public bool IsV822() { return testingType == TestingType.V822; }
        }
        public static ProgramVersion version = new ProgramVersion(); //
    }
}
