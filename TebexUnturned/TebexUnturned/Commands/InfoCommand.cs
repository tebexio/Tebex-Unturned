// using System.Collections.Generic;
// using Rocket.API;
// using Tebex.Shared.Components;
//
// namespace TebexUnturned.Commands
// {
//     public class InfoCommand : UnturnedCommand
//     {
//         public new AllowedCaller AllowedCaller => AllowedCaller.Console;
//
//         public bool RunFromConsole => true;
//
//         public new string Name => "tebex:info";
//
//         public new string Help => "Force check packages currently waiting to be executed";
//         
//         public new string Syntax => "";
//
//         public new List<string> Aliases => new List<string>();
//
//         public new List<string> Permissions => new List<string>() { "tebex.admin" };
//
//         public override void Execute(IRocketPlayer player, string[] args)
//         {
//             if (!player.HasPermission(Permissions[0]))
//             {
//                 _adapter.ReplyPlayer(player, "You do not have permission to run that command.");
//                 return;
//             }
//
//             _adapter.ReplyPlayer(player, "Getting store information...");
//             _adapter.FetchStoreInfo(info =>
//             {
//                 _adapter.ReplyPlayer(player, "Information for this server:");
//                 _adapter.ReplyPlayer(player, $"{info.ServerInfo.Name} for webstore {info.AccountInfo.Name}");
//                 _adapter.ReplyPlayer(player, $"Server prices are in {info.AccountInfo.Currency.Iso4217}");
//                 _adapter.ReplyPlayer(player, $"Webstore domain {info.AccountInfo.Domain}");
//             });
//         }
//     }
// }