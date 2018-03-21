using System;
using Rocket.API;
using System.Xml.Serialization;

namespace TebexUnturned
{
    public class TebexConfiguration : IRocketPluginConfiguration
    {
        public bool BuyEnabled = false;
        public String secret = "";
        public String baseUrl = "https://plugin.buycraft.net/";

        public void LoadDefaults()
        {
            BuyEnabled = false;
            secret = "";
            baseUrl = "https://plugin.buycraft.net/";
        }
    }
}