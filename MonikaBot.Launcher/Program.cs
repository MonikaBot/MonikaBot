using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MonikaBot.Launcher
{
    class MainClass
    {
        static ManualResetEvent resetEvent = new ManualResetEvent(false);
        static bool quitFlag = false;

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, e) => 
            {
                resetEvent.Set();
                e.Cancel = true;
            };

            Process p;
            if(File.Exists("MonikaBot.exe"))
            {
                p = new Process();
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


            while(!quitFlag)
            {
                if(p.HasExited)
                {
                    Console.WriteLine("Restarting MonikaBot..");
                    p.Start();
                }
            }
        }
    }
}
