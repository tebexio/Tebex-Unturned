using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using SDG.Unturned;
using Tebex.Shared.Components;

namespace TebexUnturned.Commands
{
    public class BanCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public bool RunFromConsole => true;

        public string Name => "tebex:ban";

        public string Help => "Bans a user from your webstore.";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>() { "tebex.ban" };

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public void Execute(IRocketPlayer commandRunner, string[] args)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            if (!_adapter.IsTebexReady())
            {
                _adapter.ReplyPlayer(commandRunner, "Tebex is not setup.");
                return;
            }

            if (!commandRunner.HasPermission(Permissions[0]))
            {
                _adapter.ReplyPlayer(commandRunner, $"Ban can only be used by administrators.");
                return;
            }

            string reason = "";
            if (args.Length == 0)
            {
                _adapter.ReplyPlayer(commandRunner, $"Usage: tebex.ban <playerName> <optional:reason>");
                return;
            }
            
            if (args.Length == 2) reason = args[1];
            var foundTargetPlayer = _adapter.GetPlayerRef(args[0].Trim()) as SteamPlayer;
            if (foundTargetPlayer == null)
            {
                _adapter.ReplyPlayer(commandRunner, $"Could not find that player on the server.");
                return;
            }

            reason = string.Join(" ", args.Skip(1));
            _adapter.ReplyPlayer(commandRunner, $"Processing ban for player {foundTargetPlayer.playerID.playerName} with reason '{reason}'");
            _adapter.BanPlayer(foundTargetPlayer.playerID.playerName, foundTargetPlayer.getAddressString(false), reason,
                (code, body) => { _adapter.ReplyPlayer(commandRunner, "Player banned successfully."); },
                error => { _adapter.ReplyPlayer(commandRunner, $"Could not ban player. {error.ErrorMessage}"); });
        }
    }
}