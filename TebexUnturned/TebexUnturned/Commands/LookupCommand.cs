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
//         private void TebexLookupCommand(IPlayer player, string command, string[] args)
//         {
//             if (!player.HasPermission(command))
//             {
//                 _adapter.ReplyPlayer(player, "You do not have permission to run that command.");
//                 return;
//             }
//
//             if (args.Length != 1)
//             {
//                 _adapter.ReplyPlayer(player, $"Usage: tebex.lookup <playerId/playerUsername>");
//                 return;
//             }
//
//             // Try to find the given player
//             var target = players.FindPlayer(args[0]);
//             if (target == null)
//             {
//                 _adapter.ReplyPlayer(player, $"Could not find a player matching the name or id {args[0]}.");
//                 return;
//             }
//
//             _adapter.GetUser(target.Id, (code, body) =>
//             {
//                 var response = JsonConvert.DeserializeObject<TebexApi.UserInfoResponse>(body);
//                 _adapter.ReplyPlayer(player, $"Username: {response.Player.Username}");
//                 _adapter.ReplyPlayer(player, $"Id: {response.Player.Id}");
//                 _adapter.ReplyPlayer(player, $"Payments Total: ${response.Payments.Sum(payment => payment.Price)}");
//                 _adapter.ReplyPlayer(player, $"Chargeback Rate: {response.ChargebackRate}%");
//                 _adapter.ReplyPlayer(player, $"Bans Total: {response.BanCount}");
//                 _adapter.ReplyPlayer(player, $"Payments: {response.Payments.Count}");
//             }, error => { _adapter.ReplyPlayer(player, error.ErrorMessage); });
//         }
//     }
// }