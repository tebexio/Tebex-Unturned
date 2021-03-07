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
    public class CommandTebexSecret : ITebexCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public bool RunFromConsole => true;

        public string Name => "tebex:secret";

        public string Help => "Set your server secret";
        
        public string Syntax => "<secret>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            String secret = command[0];
            Tebex.Instance.Configuration.Instance.secret = secret;
            Tebex.Instance.Configuration.Save();
            
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
            
            Tebex.logWarning("Your secret key has been validated! Webstore Name: " + Tebex.Instance.information.name);
        }

        public void HandleError(Exception e)
        {
            Tebex.logError("We were unable to validate your secret key.");
        }
    }
}