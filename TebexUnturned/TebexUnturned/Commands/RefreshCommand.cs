using System.Collections.Generic;
using Rocket.API;
using Tebex.Adapters;
using Tebex.API;

namespace TebexUnturned.Commands
{
    public class RefreshCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public bool RunFromConsole => true;

        public string Name => "tebex:refresh";

        public string Help => "Refreshes cached information like package and category listings";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public void Execute(IRocketPlayer commandRunner, string[] args)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            if (!commandRunner.HasPermission(Permissions[0]))
            {
                _adapter.ReplyPlayer(commandRunner, "You do not have permission to run that command.");
                return;
            }

            _adapter.ReplyPlayer(commandRunner, "Refreshing listings...");
            BaseTebexAdapter.Cache.Instance.Remove("packages");
            BaseTebexAdapter.Cache.Instance.Remove("categories");

            _adapter.RefreshListings((code, body) =>
            {
                if (BaseTebexAdapter.Cache.Instance.HasValid("packages") &&
                    BaseTebexAdapter.Cache.Instance.HasValid("categories"))
                {
                    var packs = (List<TebexApi.Package>)BaseTebexAdapter.Cache.Instance.Get("packages").Value;
                    var categories = (List<TebexApi.Category>)BaseTebexAdapter.Cache.Instance.Get("categories").Value;
                    _adapter.ReplyPlayer(commandRunner,
                        $"Fetched {packs.Count} packages out of {categories.Count} categories");
                }
            });
        }
    }
}