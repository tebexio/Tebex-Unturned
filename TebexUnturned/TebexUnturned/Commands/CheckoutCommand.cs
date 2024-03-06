using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;

namespace TebexUnturned.Commands
{
    public class CheckoutCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public bool RunFromConsole => true;

        public string Name => "tebex:checkout";

        public string Help => "Create a payment link for a package.";
        
        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public void Execute(IRocketPlayer commandRunner, string[] args)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            if (!_adapter.IsReady)
            {
                _adapter.ReplyPlayer(commandRunner, "Tebex is not setup.");
            }

            if (!commandRunner.HasPermission(Permissions[0]))
            {
                _adapter.ReplyPlayer(commandRunner, "You do not have permission to run that command.");
                return;
            }

            if (commandRunner is ConsolePlayer)
            {
                _adapter.ReplyPlayer(commandRunner,
                    $"/tebex:checkout cannot be executed via console. Use tebex:sendlink <username> <packageId> to specify a target player.");
                return;
            }

            // Only argument will be the package ID of the item in question
            if (args.Length != 1)
            {
                _adapter.ReplyPlayer(commandRunner, "Invalid syntax: Usage \"tebex:checkout <packageId>\"");
                return;
            }

            // Lookup the package by provided input and respond with the checkout URL
            var package = _adapter.GetPackageByShortCodeOrId(args[0].Trim());
            if (package == null)
            {
                _adapter.ReplyPlayer(commandRunner, "A package with that ID was not found.");
                return;
            }

            _adapter.ReplyPlayer(commandRunner, "Creating your checkout URL...");
            _adapter.CreateCheckoutUrl((commandRunner as UnturnedPlayer).SteamName, package, checkoutUrl =>
            {
                _adapter.ReplyPlayer(commandRunner, "Please visit the following URL to complete your purchase:");
                _adapter.ReplyPlayer(commandRunner, $"{checkoutUrl.Url}");
            }, error => { _adapter.ReplyPlayer(commandRunner, $"{error.ErrorMessage}"); });
        }
    }
}