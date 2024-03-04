using System.Collections.Generic;
using Rocket.API;

namespace TebexUnturned.Commands
{
    public class ForceCheckCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public bool RunFromConsole => true;

        public string Name => "tebex:forcecheck";

        public string Help => "Force check packages currently waiting to be executed";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            
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