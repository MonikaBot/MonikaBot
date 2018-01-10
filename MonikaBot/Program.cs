using System;

namespace MonikaBot
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var b = new MonikaBot())
            {
                Console.WriteLine($"Prefix: {b.config.Prefix}");
            }

            Console.ReadLine();
        }
    }
}
