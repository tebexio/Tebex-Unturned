using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using SDG.Unturned;
using Tebex.Shared.Components;

namespace TebexUnturned.Commands
{
    public class BanCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public bool RunFromConsole => true;

        public string Name => "tebex:ban";

        public string Help => "Bans a user from your webstore.";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public void Execute(IRocketPlayer commandRunner, string[] args)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            
            if (!commandRunner.HasPermission(Permissions[0]))
            {
                _adapter.ReplyPlayer(commandRunner, $"Ban can only be used by administrators.");
                return;
            }

            if (args.Length < 2)
            {
                _adapter.ReplyPlayer(commandRunner, $"Usage: tebex.ban <playerName> <reason>");
                return;
            }

            var foundTargetPlayer = _adapter.GetPlayerRef(args[0].Trim()) as SteamPlayer;
            if (foundTargetPlayer == null)
            {
                _adapter.ReplyPlayer(commandRunner, $"Could not find that player on the server.");
                return;
            }

            var reason = string.Join(" ", args.Skip(1));
            _adapter.ReplyPlayer(commandRunner, $"Processing ban for player {foundTargetPlayer.playerID.playerName} with reason '{reason}'");
            _adapter.BanPlayer(foundTargetPlayer.playerID.playerName, foundTargetPlayer.getAddressString(false), reason,
                (code, body) => { _adapter.ReplyPlayer(commandRunner, "Player banned successfully."); },
                error => { _adapter.ReplyPlayer(commandRunner, $"Could not ban player. {error.ErrorMessage}"); });
        }
    }
}