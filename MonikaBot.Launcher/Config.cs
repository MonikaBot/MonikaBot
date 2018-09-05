using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonikaBot.Launcher
{
    public class Config
    {
        public List<string> ModulesToMake = new List<string>();

        public Config()
        {
            if(!File.Exists("config.txt"))
            {
                using(StreamWriter sw = new StreamWriter("config.txt"))
                {
                    sw.WriteLine("# Place the URLS to the git modules you'd like to build and install here.");
                }
            }
        }

        /// <summary>
        /// Only have to write a read function as the user manually changes the config.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public void LoadConfigFile(string fileName)
        {
            using(StreamReader sr = new StreamReader(fileName))
            {
                string module;
                while((module = sr.ReadLine()) != null)
                {
                    if (module.StartsWith("#")) //ignore comments
                        continue;
                    
                    if (!module.EndsWith(".git"))
                        module += ".git";

                    ModulesToMake.Add(module);
                }
            }
        }
    }
}
