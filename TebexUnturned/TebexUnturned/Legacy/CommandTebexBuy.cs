// using System.Collections.Generic;
// using Rocket.API;
// using Rocket.Unturned.Player;
//
// namespace TebexUnturned.Legacy
// {
//     public class CommandTebexBuy : IRocketCommand
//     {
//         public AllowedCaller AllowedCaller => AllowedCaller.Both;
//
//         public bool RunFromConsole => true;
//
//         public string Name => "tebex:buy";
//
//         public string Help => "Buy from our webstore";
//         
//         public string Syntax => "";
//
//         public List<string> Aliases => new List<string>();
//
//         public List<string> Permissions => new List<string>() { "tebex.buy" };
//         
//         public void Execute(IRocketPlayer caller, string[] command)
//         {
//             UnturnedPlayer uPlayer = (UnturnedPlayer) caller;
//             uPlayer.Player.sendBrowserRequest(
//                 "To buy packages from our webstore, please visit: " + TebexLegacy.Instance.information.domain,
//                 TebexLegacy.Instance.information.domain);
//         } 
//     }
// }