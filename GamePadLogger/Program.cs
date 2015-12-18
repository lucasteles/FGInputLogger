using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GamePadLogger
{
    

    static class Program
    {
        public static ControlMap controller = new ControlMap();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new InputVertical());
        }
    }
}
