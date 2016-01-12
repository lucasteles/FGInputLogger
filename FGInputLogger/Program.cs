using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace FGInputLogger
{
    

    static class Program
    {
        public static ControlMap controller = new ControlMap();
        public static InputVertical MainForm ;
        public static long  milliseconds;
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm =  new  InputVertical();

           
           // Application.Idle += new EventHandler(OnApplicationIdle);
            Application.Run(MainForm);

           // milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            
        }


        private static void OnApplicationIdle(object sender, EventArgs e)
        {
            
            while (AppStillIdle)
            {
                
                var diff = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - milliseconds ;
                
                if (diff >= 1000 / 60)
                {

                    MainForm.Draw();
                  

                    milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                }


            }
        }

        private static bool AppStillIdle
        {
            get
            {
                Message msg;
                return PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }

       

        [StructLayout(LayoutKind.Sequential)]
        public struct Message
        {
            public IntPtr hWnd;
            public System.Windows.Forms.Message msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);
    }
}
