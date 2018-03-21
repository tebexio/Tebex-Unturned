using System;
using UnityEngine;
using Rocket.API;
using Rocket.Unturned.Player;
using Rocket.Unturned;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Chat;
using System.Collections.Generic;

namespace TebexUnturned
{
    public class CommandTebexBuy : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public bool RunFromConsole => true;

        public string Name => "buy";

        public string Help => "Buy from our webstore";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "tebex.buy" };
        
        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer uPlayer = (UnturnedPlayer) caller;
            uPlayer.Player.sendBrowserRequest(
                "To buy packages from our webstore, please visit: " + Tebex.Instance.information.domain,
                Tebex.Instance.information.domain);
        } 
    }
}