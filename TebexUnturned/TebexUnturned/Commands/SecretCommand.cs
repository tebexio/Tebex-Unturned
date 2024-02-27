using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Tebex.Adapters;
using Tebex.Shared.Components;

namespace TebexUnturned.Commands
{
    public class SecretCommand : UnturnedCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public bool RunFromConsole => true;

        public string Name => "tebex:secret";

        public string Help => "Connects to your webstore using the secret key.";
        
        public string Syntax => "<secretKey>";

        public List<string> Aliases => new List<string>() { "tebex.secret"};

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public override void Execute(IRocketPlayer player, string[] args)
        {
            // Secret can only be ran as the admin
            if (!player.HasPermission(Permissions[0]))
            {
                _adapter.ReplyPlayer(player, "You do not have permission to run this command.");
                _adapter.ReplyPlayer(player, "If you are an admin, grant permission to use `tebex.secret`");
                return;
            }

            if (args.Length != 1)
            {
                _adapter.ReplyPlayer(player, "Invalid syntax. Usage: \"tebex.secret <secret>\"");
                return;
            }

            _adapter.ReplyPlayer(player, "Setting your secret key...");
            BaseTebexAdapter.PluginConfig.SecretKey = args[0];
            //Config.WriteObject(BaseTebexAdapter.PluginConfig); FIXME

            // Reset store info so that we don't fetch from the cache
            BaseTebexAdapter.Cache.Instance.Remove("information");

            // Any failure to set secret key is logged to console automatically
            _adapter.FetchStoreInfo(info =>
            {
                _adapter.ReplyPlayer(player, $"Successfully set your secret key.");
                _adapter.ReplyPlayer(player,
                    $"Store set as: {info.ServerInfo.Name} for the web store {info.AccountInfo.Name}");
            });
        }
    }
}