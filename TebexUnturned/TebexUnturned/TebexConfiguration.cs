using System;
using Rocket.API;

namespace TebexUnturned
{
    public class TebexConfiguration : IRocketPluginConfiguration
    {
        public bool BuyEnabled;
        public String secret;
        public String baseUrl;

        public void LoadDefaults()
        {
            BuyEnabled = true;
            secret = "";
            baseUrl = "https://plugin.buycraft.net/";
        }        
    }
}