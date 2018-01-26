using System;

namespace MonikaBot
{
    class Program
    {
        static MonikaBot b;

        static void Main(string[] args)
        {
            if (!System.IO.Directory.Exists("modules"))
            {
                System.IO.Directory.CreateDirectory("modules");
                System.IO.Directory.CreateDirectory("modules/disabled");
            }

            if(!System.IO.Directory.Exists("modules/disabled"))
            {
                System.IO.Directory.CreateDirectory("modules/disabled");
            }

            b = new MonikaBot();
            b.ConnectBot();

            string output = "";
            while ((output = Console.ReadLine()) != null)
            {
                if (output.Trim().ToLower() == "quit")
                {
                    b.Dispose();
                    return;
                }
            }
        }

    }
}
