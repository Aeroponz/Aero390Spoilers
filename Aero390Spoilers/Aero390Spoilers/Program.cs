using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aero390Spoilers;
using System.Diagnostics;
using System.Text;
using System.Threading;
using SharedMemory;
using System.IO.Pipes;
using System.IO;

#if NET40Plus
using System.Threading.Tasks;
#endif


namespace Aero390Spoilers
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Mutex mutex = null;
            Thread.Sleep(5000);

            while (mutex == null)
            {
                try
                {
                    mutex = Mutex.OpenExisting("Global\\CSSimulation");

                }
                catch (Exception)
                {
                    Console.WriteLine("Mutex not found on " + DateTime.Now.ToString());
                    Thread.Sleep(3000);
                }


            }
            Console.WriteLine("Application2 executed on " + DateTime.Now.ToString());

            Console.WriteLine("Press <enter> to start client");
            Console.ReadLine();

            using (NamedPipeClientStream pipeClient =
            new NamedPipeClientStream(".", "ToGUI", PipeDirection.In))
            {

                // Connect to the pipe or wait until the pipe is available.
                Console.Write("Attempting to connect to pipe...");
                pipeClient.Connect();

                Console.WriteLine("Connected to pipe.");
                Console.WriteLine("There are currently {0} pipe server instances open.",
                   pipeClient.NumberOfServerInstances);
                using (StreamReader sr = new StreamReader(pipeClient))
                {
                    // Display the read text to the console
                    string temp;
                    while ((temp = sr.ReadLine()) != null)
                    {
                        Console.WriteLine("Received from server: {0}", temp);
                    }
                }
            }
            Console.Write("Press Enter to continue...");
            Console.ReadLine();



            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AircraftGUI());
            Console.ReadLine();
        }
    }
}
