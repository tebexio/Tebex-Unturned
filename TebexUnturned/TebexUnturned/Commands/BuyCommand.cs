using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;
using Tebex.Adapters;
using Tebex.API;

namespace TebexUnturned.Commands
{
    public class BuyCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public bool RunFromConsole => true;

        public string Name => "tebex:buy";

        public string Help => "Opens the webstore.";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>() { "tebex.buy"};

        public List<string> Permissions => new List<string>() { };

        public void Execute(IRocketPlayer commandRunner, string[] args)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            if (!_adapter.IsReady)
            {
                _adapter.ReplyPlayer(commandRunner, "Tebex is not setup.");
                return;
            }

            if (!BaseTebexAdapter.PluginConfig.BuyEnabled)
            {
                _adapter.ReplyPlayer(commandRunner, "Buying is not enabled.");
                return;
            }
            
            if (commandRunner is ConsolePlayer)
            {
                _adapter.ReplyPlayer(commandRunner,
                    $"/tebex:buy cannot be executed via console. Use tebex:sendlink <username> <packageId> to specify a target player.");
                return;
            }

            if (commandRunner is UnturnedPlayer)
            {
                var player = (UnturnedPlayer)commandRunner;
                _adapter.LogInfo($"Buy command received from 'steam:{player.SteamName}/ign:{player.DisplayName}'");
                if (BaseTebexAdapter.Cache.Instance.HasValid("information"))
                {
                    TebexApi.TebexStoreInfo storeInfo = (TebexApi.TebexStoreInfo)BaseTebexAdapter.Cache.Instance.Get("information").Value;
                    player.Player.sendBrowserRequest(
                        "To buy packages from our webstore, please visit: " + storeInfo.AccountInfo.Domain,
                        storeInfo.AccountInfo.Domain);      
                }
                else
                {
                    _adapter.LogError("Store information is not available. Check secret key and run /tebex:refresh");
                    _adapter.ReplyPlayer(player, "This store is not yet setup.");
                }
            }
        }
    }
}