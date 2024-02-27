// using System.Collections.Generic;
// using Rocket.API;
//
// namespace TebexUnturned.Commands
// {
//     public class ForceCheckCommand : IRocketCommand
//     {
//         public AllowedCaller AllowedCaller => AllowedCaller.Console;
//
//         public bool RunFromConsole => true;
//
//         public string Name => "tebex:secret";
//
//         public string Help => "Force check packages currently waiting to be executed";
//         
//         public string Syntax => "";
//
//         public List<string> Aliases => new List<string>();
//
//         public List<string> Permissions => new List<string>() { "tebex.admin" };
//
//         public void Execute(IRocketPlayer caller, string[] command)
//         {
//             
//         }
//         
//         private void TebexCheckoutCommand(IPlayer player, string command, string[] args)
//         {
//             if (!player.HasPermission(command))
//             {
//                 _adapter.ReplyPlayer(player, "You do not have permission to run that command.");
//                 return;
//             }
//
//             if (player.IsServer)
//             {
//                 _adapter.ReplyPlayer(player,
//                     $"{command} cannot be executed via server. Use tebex:sendlink <username> <packageId> to specify a target player.");
//                 return;
//             }
//
//             // Only argument will be the package ID of the item in question
//             if (args.Length != 1)
//             {
//                 _adapter.ReplyPlayer(player, "Invalid syntax: Usage \"tebex.checkout <packageId>\"");
//                 return;
//             }
//
//             // Lookup the package by provided input and respond with the checkout URL
//             var package = _adapter.GetPackageByShortCodeOrId(args[0].Trim());
//             if (package == null)
//             {
//                 _adapter.ReplyPlayer(player, "A package with that ID was not found.");
//                 return;
//             }
//
//             _adapter.ReplyPlayer(player, "Creating your checkout URL...");
//             _adapter.CreateCheckoutUrl(player.Name, package, checkoutUrl =>
//             {
//                 player.Command("chat.add", 0, player.Id, "Please visit the following URL to complete your purchase:");
//                 player.Command("chat.add", 0, player.Id, $"{checkoutUrl.Url}");
//             }, error => { _adapter.ReplyPlayer(player, $"{error.ErrorMessage}"); });
//         }
//     }
// }