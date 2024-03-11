using System.Collections.Generic;
using Rocket.API;
using Tebex.Shared.Components;

namespace TebexUnturned.Commands
{
    public class InfoCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public bool RunFromConsole => true;

        public string Name => "tebex:info";

        public string Help => "Shows information about this server's store.";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { };

        public void Execute(IRocketPlayer player, string[] args)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            if (!_adapter.IsReady)
            {
                _adapter.ReplyPlayer(player, "Tebex is not setup.");
            }
            
            _adapter.ReplyPlayer(player, "Getting store information...");
            _adapter.FetchStoreInfo(info =>
            {
                _adapter.ReplyPlayer(player, "Information for this server:");
                _adapter.ReplyPlayer(player, $"{info.ServerInfo.Name} for webstore {info.AccountInfo.Name}");
                _adapter.ReplyPlayer(player, $"Server prices are in {info.AccountInfo.Currency.Iso4217}");
                _adapter.ReplyPlayer(player, $"Webstore domain {info.AccountInfo.Domain}");
            });
        }
    }
}