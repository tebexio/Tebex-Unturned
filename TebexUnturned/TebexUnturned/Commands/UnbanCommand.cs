using System.Collections.Generic;
using Rocket.API;

namespace TebexUnturned.Commands
{
    public class UnbanCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public bool RunFromConsole => true;

        public string Name => "tebex:unban";

        public string Help => "Unbans a player from the webstore.";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>() { "tebex.unban"};

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public void Execute(IRocketPlayer commandRunner, string[] args)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            if (!_adapter.IsReady)
            {
                _adapter.ReplyPlayer(commandRunner, "Tebex is not setup.");
                return;
            }

            if (!commandRunner.IsAdmin)
            {
                _adapter.ReplyPlayer(commandRunner, $"/tebex:unban can only be used by administrators.");
                return;
            }
            
            _adapter.ReplyPlayer(commandRunner, $"You must unban players via your webstore.");
        }
    }
}