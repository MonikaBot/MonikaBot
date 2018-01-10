using Newtonsoft.Json;
using System;
using System.IO;

namespace MonikaBot
{
    internal class MonikaBotConfig
    {
        [JsonProperty("token")]
        internal string Token = "Bot user Token here...";

        /// <summary>
        /// The prefix is how you summon the bot, a character or bit of text that's before each message that lets the bot know that this is going to be for it.
        /// </summary>
        [JsonProperty("prefix")]
        internal string Prefix = "--";

        public MonikaBotConfig LoadConfig(string path)
        {
            using (var sr = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<MonikaBotConfig>(sr.ReadToEnd());
            }
        }

        public void WriteConfig(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                sw.Write(JsonConvert.SerializeObject(this));
            }
        }
    }
}
