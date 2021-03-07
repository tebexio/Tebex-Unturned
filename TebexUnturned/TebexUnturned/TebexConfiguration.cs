using System;
using Rocket.API;

namespace TebexUnturned
{
    public class TebexConfiguration : IRocketPluginConfiguration
    {
        public bool BuyEnabled;
        public int CheckIntervalInSeconds;
        public String BuyCommand;
        public String secret;
        public String baseUrl;
        public void LoadDefaults()
        {
            BuyEnabled = true;
            secret = "";
            BuyCommand = "!donate";
            baseUrl = "https://plugin.buycraft.net/";
            CheckIntervalInSeconds = 60;
        }
    }
}