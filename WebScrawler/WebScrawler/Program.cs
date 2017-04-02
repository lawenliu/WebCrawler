using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebScrawler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// args[0]: Input file path, args[1] Output folder
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length != 2)
            {
                return;   
            }

            Application.Run(new MainWindow(args[0], args[1]));
        }
    }
}
