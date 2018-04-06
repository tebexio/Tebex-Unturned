using System;
using Rocket.API;
using System.Xml.Serialization;

namespace TebexUnturned
{
    public class TebexConfiguration : IRocketPluginConfiguration
    {
        public bool BuyEnabled = true;
        public String BuyCommand = "!donate";
        public String secret = "";
        public String baseUrl = "https://plugin.buycraft.net/";

        public void LoadDefaults()
        {
            BuyEnabled = true;
            secret = "";
            BuyCommand = "!donate";
            baseUrl = "https://plugin.buycraft.net/";
        }
    }
}
