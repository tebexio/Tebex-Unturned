using System.Collections.Generic;
using Rocket.API;
using Tebex.Shared.Components;

namespace TebexUnturned.Commands
{
    public class HelpCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public bool RunFromConsole => true;

        public string Name => "tebex:help";

        public string Help => "Shows available commands.";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>() { "tebex.help" };

        public List<string> Permissions => new List<string>() { };

        public HelpCommand() {}
        
        public void Execute(IRocketPlayer player, string[] command)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            if (!_adapter.IsReady)
            {
                _adapter.ReplyPlayer(player, "Tebex is not setup.");
            }

            _adapter.ReplyPlayer(player, "Tebex Commands Available:");
            if (player.IsAdmin || player.HasPermission("tebex:admin")) //Always show help to admins regardless of perms, for new server owners
            {
                _adapter.ReplyPlayer(player, "-- Administrator Commands --");
                _adapter.ReplyPlayer(player, "tebex:secret <secretKey>          - Sets your server's secret key.");
                _adapter.ReplyPlayer(player, "tebex:debug <on/off>              - Enables or disables debug logging.");
                _adapter.ReplyPlayer(player,
                    "tebex:sendlink <player> <packId>  - Sends a purchase link to the provided player.");
                _adapter.ReplyPlayer(player,
                    "tebex:forcecheck                  - Forces the command queue to check for any pending purchases.");
                _adapter.ReplyPlayer(player,
                    "tebex:refresh                     - Refreshes store information, packages, categories, etc.");
                _adapter.ReplyPlayer(player,
                    "tebex:report                      - Generates a report for the Tebex support team.");
                _adapter.ReplyPlayer(player,
                    "tebex:ban <playerId>              - Bans a player from using your Tebex store.");
                _adapter.ReplyPlayer(player,
                    "tebex:lookup <playerId>           - Looks up store statistics for the given player.");
            }

            _adapter.ReplyPlayer(player, "-- User Commands --");
            _adapter.ReplyPlayer(player,
                "tebex:info                       - Get information about this server's store.");
            _adapter.ReplyPlayer(player,
                "tebex:categories                 - Shows all item categories available on the store.");
            _adapter.ReplyPlayer(player,
                "tebex:packages <opt:categoryId>  - Shows all item packages available in the store or provided category.");
            _adapter.ReplyPlayer(player,
                "tebex:checkout <packId>          - Creates a checkout link for an item. Visit to purchase.");
            _adapter.ReplyPlayer(player,
                "tebex:buy                        - Opens this server's webstore.");
        }
    }
}