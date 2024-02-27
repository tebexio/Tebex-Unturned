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
//         private void TebexRefreshCommand(IPlayer player, string command, string[] args)
//         {
//             if (!player.HasPermission(command))
//             {
//                 _adapter.ReplyPlayer(player, "You do not have permission to run that command.");
//                 return;
//             }
//
//             _adapter.ReplyPlayer(player, "Refreshing listings...");
//             BaseTebexAdapter.Cache.Instance.Remove("packages");
//             BaseTebexAdapter.Cache.Instance.Remove("categories");
//
//             _adapter.RefreshListings((code, body) =>
//             {
//                 if (BaseTebexAdapter.Cache.Instance.HasValid("packages") &&
//                     BaseTebexAdapter.Cache.Instance.HasValid("categories"))
//                 {
//                     var packs = (List<TebexApi.Package>)BaseTebexAdapter.Cache.Instance.Get("packages").Value;
//                     var categories = (List<TebexApi.Category>)BaseTebexAdapter.Cache.Instance.Get("categories").Value;
//                     _adapter.ReplyPlayer(player,
//                         $"Fetched {packs.Count} packages out of {categories.Count} categories");
//                 }
//             });
//         }
//     }
// }