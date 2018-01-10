using DSharpPlus;
using System;

namespace MonikaBot
{
    class MonikaBot : IDisposable
    {
        private DiscordClient _client;

        public MonikaBot()
        {

        }

        public void Dispose()
        {
            //nothing yet, we have nothing to dispose!
        }
    }
}
