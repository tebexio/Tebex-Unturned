using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;

namespace TebexUnturned.Commands
{
    public class SendLinkCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public bool RunFromConsole => true;

        public string Name => "tebex:sendlink";

        public string Help => "Force check packages currently waiting to be executed";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public void Execute(IRocketPlayer commandRunner, string[] args)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            if (!_adapter.IsReady)
            {
                _adapter.ReplyPlayer(commandRunner, "Tebex is not setup.");
            }

            if (!commandRunner.HasPermission("tebex.sendlink"))
            {
                _adapter.ReplyPlayer(commandRunner, "You must be an administrator to run this command.");
                return;
            }

            if (args.Length != 2)
            {
                _adapter.ReplyPlayer(commandRunner, "Usage: tebex.sendlink <username> <packageId>");
                return;
            }

            var username = args[0].Trim();
            var package = _adapter.GetPackageByShortCodeOrId(args[1].Trim());
            if (package == null)
            {
                _adapter.ReplyPlayer(commandRunner, "A package with that ID was not found.");
                return;
            }

            _adapter.ReplyPlayer(commandRunner,
                $"Creating checkout URL with package '{package.Name}'|{package.Id} for player {username}");
            var player = _adapter.GetPlayerRef(username) as UnturnedPlayer;
            if (player == null)
            {
                _adapter.ReplyPlayer(commandRunner, $"Couldn't find that player on the server.");
                return;
            }

            _adapter.CreateCheckoutUrl(player.SteamName, package, checkoutUrl =>
            {
                _adapter.ReplyPlayer(player, "Please visit the following URL to complete your purchase:");
                _adapter.ReplyPlayer(player, $"{checkoutUrl.Url}");
            }, error => { _adapter.ReplyPlayer(player, $"{error.ErrorMessage}"); });
        }
    }
}