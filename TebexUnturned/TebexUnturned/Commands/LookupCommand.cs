using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Rocket.API;
using Rocket.Unturned.Player;
using Tebex.API;

namespace TebexUnturned.Commands
{
    public class LookupCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public bool RunFromConsole => true;

        public string Name => "tebex:lookup";

        public string Help => "Force check packages currently waiting to be executed";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>() { "tebex.lookup" };

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public void Execute(IRocketPlayer commandRunner, string[] args)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            if (!_adapter.IsReady)
            {
                _adapter.ReplyPlayer(commandRunner, "Tebex is not setup.");
                return;
            }

            if (!commandRunner.HasPermission(Permissions[0]))
            {
                _adapter.ReplyPlayer(commandRunner, "You do not have permission to run that command.");
                return;
            }

            if (args.Length != 1)
            {
                _adapter.ReplyPlayer(commandRunner, $"Usage: tebex.lookup <playerId/playerUsername>");
                return;
            }

            // Try to find the given player
            var target = _adapter.GetPlayerRef(args[0]) as UnturnedPlayer;
            if (target == null)
            {
                _adapter.ReplyPlayer(commandRunner, $"Could not find a player matching the name or id {args[0]}.");
                return;
            }

            _adapter.GetUser(target.SteamProfile.SteamID, (code, body) =>
            {
                var response = JsonConvert.DeserializeObject<TebexApi.UserInfoResponse>(body);
                _adapter.ReplyPlayer(commandRunner, $"Username: {response.Player.Username}");
                _adapter.ReplyPlayer(commandRunner, $"Id: {response.Player.Id}");
                _adapter.ReplyPlayer(commandRunner, $"Payments Total: ${response.Payments.Sum(payment => payment.Price)}");
                _adapter.ReplyPlayer(commandRunner, $"Chargeback Rate: {response.ChargebackRate}%");
                _adapter.ReplyPlayer(commandRunner, $"Bans Total: {response.BanCount}");
                _adapter.ReplyPlayer(commandRunner, $"Payments: {response.Payments.Count}");
            }, error => { _adapter.ReplyPlayer(commandRunner, error.ErrorMessage); });
        }
    }
}