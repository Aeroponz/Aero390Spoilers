using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
#if NET40Plus
using System.Threading.Tasks;
#endif

namespace CSSimulationNode
{
    class Program
    {
        private static bool isNewMutexCreated = true;
        private static Mutex mutex;

        static void Main(string[] args)
        {
            mutex = new Mutex(true, "Global\\CSSimulation", out isNewMutexCreated);
            //AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            Console.WriteLine("Application1 executed on " + DateTime.Now.ToString());
            Console.WriteLine("STARTING SIMULATION SERVER...");
            //Console.ReadLine();

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

            Console.WriteLine("End of Main(), time to autodestruct!");
            Console.ReadLine();
        }

        static void CurrentDomain_ProcessExit(Object sender, EventArgs e)
        {
            if (isNewMutexCreated)
            {
                Console.WriteLine("Mutex Released");
                mutex.ReleaseMutex();
            }
        }
    }
}
