using System.Collections.Generic;
using Rocket.API;

namespace TebexUnturned.Commands
{
    public class ForceCheckCommand : UnturnedCommand
    {
        public new AllowedCaller AllowedCaller => AllowedCaller.Console;

        public bool RunFromConsole => true;

        public new string Name => "tebex:forcecheck";

        public new string Help => "Force check packages currently waiting to be executed";
        
        public new string Syntax => "";

        public new List<string> Aliases => new List<string>();

        public new List<string> Permissions => new List<string>() { "tebex.admin" };

        public override void Execute(IRocketPlayer caller, string[] command)
        {
            if (!caller.HasPermission(Permissions[0]))
            {
                _adapter.ReplyPlayer(caller, "You do not have permission to run that command.");
                return;
            }

            _adapter.RefreshStoreInformation(true);
            _adapter.ProcessCommandQueue(true);
            _adapter.ProcessJoinQueue(true);
            _adapter.DeleteExecutedCommands(true);
        }
    }
}