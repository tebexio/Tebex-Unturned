using System;
using UnityEngine;
using Rocket.API;
using Rocket.Unturned.Player;
using Rocket.Unturned;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Chat;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TebexUnturned
{
    public class CommandTebexForcecheck : ITebexCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public bool RunFromConsole => true;

        public string Name => "tebex:forcecheck";

        public string Help => "Force check packages currently waiting to be executed";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public void Execute(IRocketPlayer caller, string[] command)
        {           
            try
            {               
                TebexApiClient wc = new TebexApiClient();
                wc.setPlugin(Tebex.Instance);
                wc.DoGet("queue", this);
                wc.Dispose();
            }
            catch (TimeoutException)
            {
                Tebex.logWarning("Timeout!");
            }
        }

        public void HandleResponse(JObject response)
        {
            if ((bool) response["meta"]["execute_offline"])
            {
                TebexCommandRunner.doOfflineCommands();
                
                Tebex.logWarning("Continue....");
            }
        }

        public void HandleError(Exception e)
        {
            Tebex.logError("We are unable to fetch your server queue. Please check your secret key.");
            Tebex.logError(e.ToString());
        }         
    }
}