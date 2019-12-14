using ARINC;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

#if NET40Plus
using System.Threading.Tasks;
#endif


namespace Aero390Spoilers
{
    static class Program
    {
        private static System.Timers.Timer aTimer;
        private static ConsoleSpiner Spin = new ConsoleSpiner();

        public static NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "ToGUI", PipeDirection.In);
        public static StreamReader srToGUI = new StreamReader(pipeClient);
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
            //Console.WriteLine("Press <enter> to start client");
            //Console.ReadLine();

            Console.WriteLine("\n\n----- STARTING AIRCRAFT CLIENT -----\n\n");

            // Connect to the pipe or wait until the pipe is available.
            Console.Write("Attempting to connect to pipe...");
            pipeClient.Connect();

            Console.WriteLine("Connected to pipe.");
            Console.WriteLine("There are currently {0} pipe server instances open.", pipeClient.NumberOfServerInstances);

            Console.WriteLine("\n\n----- STARTING AIRCRAFT GUI -----\n");
            System.Threading.Thread AircraftGUIThread = new System.Threading.Thread(new System.Threading.ThreadStart(Program.RunGUI));
            AircraftGUIThread.Start();

            Console.WriteLine("----- AIRCRAFT GUI RUNNING -----\n");

            SetTimer();
            Console.ReadLine();
            aTimer.Stop();
            aTimer.Dispose();
        }
        private static void RunGUI()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AircraftGUI());
        }

        public class ConsoleSpiner
        {
            int counter;
            public ConsoleSpiner()
            {
                counter = 0;
            }
            public void Turn()
            {
                counter++;
                switch (counter)
                {
                    case 0: Console.Write("           -----|-----\n        *>=====[_]L)\n              - '-`-"); break;
                    case 1: Console.Write("             ---|---  \n        *>=====[_]L)\n              - '-`-"); break;
                    case 2: Console.Write("              --|--   \n        *>=====[_]L)\n              - '-`-"); break;
                    case 3: Console.Write("                |     \n        *>=====[_]L)\n              - '-`-"); break;
                    case 4: Console.Write("              --|--   \n        *>=====[_]L)\n              - '-`-"); break;
                    case 5: Console.Write("             ---|---  \n        *>=====[_]L)\n              - '-`-"); break;
                    case 6: Console.Write("           -----|-----\n        *>=====[_]L)\n              - '-`-"); counter = -1; break;
                    default: break;
                }
                Console.SetCursorPosition(0, 15);
            }
        }
        //Helicopter Timer
        private static void SetTimer()
        {
            // Create a timer with a second interval.
            aTimer = new System.Timers.Timer(200);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        //Tells timer what to do when delay is met
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //Spin.Turn();
            // Display the read text to the console
            string temp;
            if ((temp = srToGUI.ReadLine()) != null)
            {
                Console.WriteLine("Received from server: {0}", temp);
                if (temp != "Connection Successful!")
                {
                    ARINCMessage received = new ARINCMessage();
                    //received.ToBitArray(temp);
                    Console.WriteLine(temp);
                }
            }

        }
    }
}
