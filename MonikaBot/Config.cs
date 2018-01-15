using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;

namespace MonikaBot
{
    internal class MonikaBotConfig
    {
        public static string BlankTokenString = "Bot user Token here...";
        public static string BlankOwnerIDString = "NONE";

        [JsonProperty("token")]
        internal string Token = BlankTokenString;

        /// <summary>
        /// The prefix is how you summon the bot, a character or bit of text that's before each message that lets the bot know that this is going to be for it.
        /// </summary>
        [JsonProperty("prefix")]
        internal string Prefix = "--";

        /// <summary>
        /// The owner identifier.
        /// </summary>
        [JsonProperty("ownerid")]
        internal string OwnerID = "NONE";

        /// <summary>
        /// Makes the bot respond to a mention instead of a prefix.
        /// </summary>
        [JsonProperty("respondbymention")]
        internal bool RespondByMention = false;

        /// <summary>
        /// A dictionary containing the following
        /// - Name of the module
        /// - True/False if enabled or not.
        /// </summary>
        /// <value>The modules dictionary.</value>
        [JsonProperty("modules")]
        public Dictionary<string, bool> ModulesDictionary { get; internal set; } //null will mean all enabled

        public MonikaBotConfig LoadConfig(string path = "config.json")
        {
            using (var sr = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<MonikaBotConfig>(sr.ReadToEnd());
            }
        }

        public void WriteConfig(string path = "config.json")
        {
            using (var sw = new StreamWriter(path))
            {
                sw.Write(JsonConvert.SerializeObject(this));
            }
        }
    }
}
