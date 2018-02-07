using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MonikaBot.Launcher
{
    class MainClass
    {
        static ManualResetEvent resetEvent = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, e) => 
            {
                resetEvent.Set();
                e.Cancel = true;
            };

            if(File.Exists("MonikaBot.exe"))
            {
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo("MonikaBot.exe");
                p.OutputDataReceived += (sender, e) => 
                {
                    Console.WriteLine("Data received");
                };

                p.Start();
            }
            else
            {
                Console.WriteLine("Couldn't find MonikaBot.exe in current directory!");

                return;
            }

            resetEvent.WaitOne();
        }
    }
}
