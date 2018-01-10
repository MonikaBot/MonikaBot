using System;

namespace MonikaBot
{
    class Program
    {
        static void Main(string[] args)
        {
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
