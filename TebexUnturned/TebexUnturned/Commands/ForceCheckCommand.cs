using System.Collections.Generic;
using Rocket.API;

namespace TebexUnturned.Commands
{
    public class ForceCheckCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public bool RunFromConsole => true;

        public string Name => "tebex:forcecheck";

        public string Help => "Force check packages currently waiting to be executed";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>() { "tebex.forcecheck" };

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public void Execute(IRocketPlayer player, string[] command)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            if (!_adapter.IsReady)
            {
                _adapter.ReplyPlayer(player, "Tebex is not setup.");
            }

            if (!player.HasPermission(Permissions[0]))
            {
                _adapter.ReplyPlayer(player, "You do not have permission to run that command.");
                return;
            }

            _adapter.RefreshStoreInformation(true);
            _adapter.ProcessCommandQueue(true);
            _adapter.ProcessJoinQueue(true);
            _adapter.DeleteExecutedCommands(true);
        }
    }
}