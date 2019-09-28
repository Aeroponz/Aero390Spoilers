using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Timers;

namespace CSSimulationNode
{
    static class Program
    {
        private static bool isNewMutexCreated = true;
        private static Mutex mutex;
        private static System.Timers.Timer aTimer;
        public static int SimulationTicks = 0;
        public static int UpdateTicks = 0;
        public static int TotalTicks = 0;
        public static Ownship.Aircraft Ownship;

        static void Main(string[] args)
        {
            mutex = new Mutex(true, "Global\\CSSimulation", out isNewMutexCreated);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            Console.WriteLine("Application1 executed on " + DateTime.Now.ToString());
            Console.WriteLine("\n\n----- STARTING SIMULATION SERVER -----\n\n");
            //Console.ReadLine();

            //Initialise NamedPipe and send test string to clients
            using (NamedPipeServerStream pipeServer =
            new NamedPipeServerStream("ToGUI", PipeDirection.Out))
            {
                Console.WriteLine("ToGUI NamedPipeServerStream object created.");

                // Wait for a client to connect
                Console.Write("Waiting for client connection...");
                pipeServer.WaitForConnection();

                Console.WriteLine("Client connected.");
                try
                {
                    // Read user input and send that to the client process.
                    using (StreamWriter sw = new StreamWriter(pipeServer))
                    {
                        sw.AutoFlush = true;
                        //Console.Write("Enter text: ");
                        //sw.WriteLine(Console.ReadLine());
                        sw.WriteLine("Connection Test... 1... 2...");
                    }
                }
                // Catch the IOException that is raised if the pipe is broken
                // or disconnected.
                catch (IOException e)
                {
                    Console.WriteLine("ERROR: {0}", e.Message);
                }

            }

            //Pipe created successfully, instantiate aircraft
            Console.Write("Manufacturing Aircraft (Object Creation)... " );
            Ownship = new Ownship.Aircraft();
            Console.WriteLine("Aircraft Ready!\n");

            //Aircraft Instantiated, start simulation (Tick Clock)
            SetTimer();

            //Stop Simulation and Clean Solution
            Console.WriteLine("Simulation Loaded at {0:HH:mm:ss.fff}", DateTime.Now);
            Console.WriteLine("\nPress the Enter key to exit the application...\n");
            Console.ReadLine();
            aTimer.Stop();
            aTimer.Dispose();
            Console.WriteLine("Simulation unloaded...");
            Console.WriteLine("Load Time: " + SimulationTicks.ToString() + " Ticks ~= " + (SimulationTicks/4).ToString() + " seconds\n");
            Console.WriteLine("End of Main(), time to selfdestruct!");
            Console.ReadLine();
        }

        //Sends exception if Server goes down before clients
        static void CurrentDomain_ProcessExit(Object sender, EventArgs e)
        {
            if (isNewMutexCreated)
            {
                Console.WriteLine("Mutex Released");
            }
        }

        //This method is called at every second interval.
        public static void Simulation_Tick()
        {
            //TO DO: CREATE TICK FOR EACH SYSTEM TO BE CALLED HERE
        }
        //This method is called at every second interval.
        public static void SendUpdateToGUI_Tick()
        {
            //TO DO: ARINC ENCODER
            //TO DO: SEND ENCODED DATA USING ToGUI NamedPipe
        }
        //Instantiate Simulation Timer
        private static void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(250);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        //Tells timer what to do when delay is met
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if(TotalTicks % 2 == 0)//Tick Simulation
            {
                //Console.WriteLine("Simulation Tick at {0:HH:mm:ss.fff}", e.SignalTime);
                Simulation_Tick();
                SimulationTicks++;
                TotalTicks++;
            }
            else//Tick Update
            {
                //Console.WriteLine("Update Tick at {0:HH:mm:ss.fff}", e.SignalTime);
                SendUpdateToGUI_Tick();
                UpdateTicks++;
                TotalTicks++;
            }
        }
    }
}
