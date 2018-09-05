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

        private static bool Setup = false;
        private static bool LaunchAfter = true;

        private static Config LauncherConfig;

        public static string GitModulesPath;

        private bool GitClone(string urlToClone)
        {
            // So we don't clutter wherever MonikaBot is located.
            if (!Directory.Exists("./git/"))
            {
                Directory.CreateDirectory("./git/");
            }

            using (Process p = new Process())
            {
                p.StartInfo = new ProcessStartInfo { FileName = "git", Arguments = $"clone {urlToClone}", WorkingDirectory = "./git/" };
            }

            return false;
        }

        private static bool HaveAllPrograms()
        {
            string[] ProgramCheckList = { "msbuild", "git" };

            foreach (var programName in ProgramCheckList)
            {
                try
                {
                    Console.WriteLine(programName.Bash());
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            return true;
        }

        public static void Main(string[] args)
        {
            GitModulesPath = Path.Combine(Environment.CurrentDirectory, "git");

            LauncherConfig = new Config();



            if (HaveAllPrograms())
            {
                Console.WriteLine("All set");
            }
            else
                Console.WriteLine("Nope ;/");

            Console.ReadLine();

#if TEST
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
#endif
        }
    }
}
