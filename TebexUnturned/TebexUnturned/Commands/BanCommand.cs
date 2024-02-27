// using System.Collections.Generic;
// using System.Linq;
// using Rocket.API;
// using Tebex.Shared.Components;
//
// namespace TebexUnturned.Commands
// {
//     public class BanCommand : IRocketCommand
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
//         private void TebexBanCommand(IPlayer commandRunner, string command, string[] args)
//         {
//             if (!commandRunner.HasPermission("tebex.ban"))
//             {
//                 _adapter.ReplyPlayer(commandRunner, $"{command} can only be used by administrators.");
//                 return;
//             }
//
//             if (args.Length < 2)
//             {
//                 _adapter.ReplyPlayer(commandRunner, $"Usage: tebex.ban <playerName> <reason>");
//                 return;
//             }
//
//             var player = players.FindPlayer(args[0].Trim());
//             if (player == null)
//             {
//                 _adapter.ReplyPlayer(commandRunner, $"Could not find that player on the server.");
//                 return;
//             }
//
//             var reason = string.Join(" ", args.Skip(1));
//             _adapter.ReplyPlayer(commandRunner, $"Processing ban for player {player.Name} with reason '{reason}'");
//             _adapter.BanPlayer(player.Name, player.Address, reason,
//                 (code, body) => { _adapter.ReplyPlayer(commandRunner, "Player banned successfully."); },
//                 error => { _adapter.ReplyPlayer(commandRunner, $"Could not ban player. {error.ErrorMessage}"); });
//         }
//     }
// }