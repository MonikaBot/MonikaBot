using System;

namespace MonikaBot
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!System.IO.Directory.Exists("modules"))
                System.IO.Directory.CreateDirectory("modules");

            MonikaBot b = new MonikaBot();
            b.ConnectBot();

            string output = "";
            while ((output = Console.ReadLine()) != null)
            {
                if (output == "")
                {
                    return;
                }
            }

        }

    }
}
