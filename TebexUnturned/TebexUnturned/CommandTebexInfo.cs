using System;
using UnityEngine;
using Rocket.API;
using Rocket.Unturned.Player;
using Rocket.Unturned;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Chat;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TebexUnturned
{
    public class CommandTebexInfo : ITebexCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public bool RunFromConsole => true;

        public string Name => "tebex:info";

        public string Help => "Get basic details about the webstore";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public void Execute(IRocketPlayer caller, string[] command)
        {           
            try
            {               
                TebexApiClient wc = new TebexApiClient();
                wc.setPlugin(Tebex.Instance);
                wc.DoGet("information", this);
                wc.Dispose();
            }
            catch (TimeoutException)
            {
                Tebex.logWarning("Timeout!");
            }
        }

        public void HandleResponse(JObject response)
        {
            Tebex.Instance.information.id = (int) response["account"]["id"];
            Tebex.Instance.information.domain = (string) response["account"]["domain"];
            Tebex.Instance.information.gameType = (string) response["account"]["game_type"];
            Tebex.Instance.information.name = (string) response["account"]["name"];
            Tebex.Instance.information.currency = (string) response["account"]["currency"]["iso_4217"];
            Tebex.Instance.information.currencySymbol = (string) response["account"]["currency"]["symbol"];
            Tebex.Instance.information.serverId = (int) response["server"]["id"];
            Tebex.Instance.information.serverName = (string) response["server"]["name"];
            
            Tebex.logWarning("Server Information");
            Tebex.logWarning("=================");
            Tebex.logWarning("Server "+Tebex.Instance.information.serverName+" for webstore "+Tebex.Instance.information.name+"");
            Tebex.logWarning("Server prices are in "+Tebex.Instance.information.currency+"");
            Tebex.logWarning("Webstore domain: "+Tebex.Instance.information.domain+"");
        }

        public void HandleError(Exception e)
        {
            Tebex.logError("We are unable to fetch your server details. Please check your secret key.");
        }      
    }
}