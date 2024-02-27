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
//         private void TebexSendLinkCommand(IPlayer commandRunner, string command, string[] args)
//         {
//             if (!commandRunner.HasPermission("tebex.sendlink"))
//             {
//                 _adapter.ReplyPlayer(commandRunner, "You must be an administrator to run this command.");
//                 return;
//             }
//
//             if (args.Length != 2)
//             {
//                 _adapter.ReplyPlayer(commandRunner, "Usage: tebex.sendlink <username> <packageId>");
//                 return;
//             }
//
//             var username = args[0].Trim();
//             var package = _adapter.GetPackageByShortCodeOrId(args[1].Trim());
//             if (package == null)
//             {
//                 _adapter.ReplyPlayer(commandRunner, "A package with that ID was not found.");
//                 return;
//             }
//
//             _adapter.ReplyPlayer(commandRunner,
//                 $"Creating checkout URL with package '{package.Name}'|{package.Id} for player {username}");
//             var player = players.FindPlayer(username);
//             if (player == null)
//             {
//                 _adapter.ReplyPlayer(commandRunner, $"Couldn't find that player on the server.");
//                 return;
//             }
//
//             _adapter.CreateCheckoutUrl(player.Name, package, checkoutUrl =>
//             {
//                 player.Command("chat.add", 0, player.Id, "Please visit the following URL to complete your purchase:");
//                 player.Command("chat.add", 0, player.Id, $"{checkoutUrl.Url}");
//             }, error => { _adapter.ReplyPlayer(player, $"{error.ErrorMessage}"); });
//         }
//     }
// }