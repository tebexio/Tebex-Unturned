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
//         private void TebexCategoriesCommand(IPlayer player, string command, string[] args)
//         {
//             if (!player.HasPermission(command))
//             {
//                 _adapter.ReplyPlayer(player, "You do not have permission to run that command.");
//                 return;
//             }
//
//             _adapter.GetCategories(categories => { PrintCategories(player, categories); });
//         }
//     }
// }