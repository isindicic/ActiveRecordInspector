using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SindaSoft.ActiveRecordInspector
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] argv)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm mf = new MainForm();
            if (argv.Length == 1)
                mf.path2investigate = argv[0];

            Application.Run(mf);
        }
    }
}
