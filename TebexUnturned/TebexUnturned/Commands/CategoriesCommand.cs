using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;

namespace TebexUnturned.Commands
{
    public class CategoriesCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public bool RunFromConsole => true;

        public string Name => "tebex:categories";

        public string Help => "Print available package categories.";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>() { "tebex.categories" };

        public List<string> Permissions => new List<string>() { };

        public void Execute(IRocketPlayer commandRunner, string[] command)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            if (!_adapter.IsTebexReady())
            {
                _adapter.ReplyPlayer(commandRunner, "Tebex is not setup.");
                return;
            }

            _adapter.GetCategories(categories => { Tebex.Plugins.TebexUnturned.PrintCategories(commandRunner, categories); });
        }
    }
}