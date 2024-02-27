// using System;
// using System.Collections.Generic;
// using Newtonsoft.Json.Linq;
// using Rocket.API;
// using Rocket.Unturned.Player;
// using Steamworks;
// using TebexUnturned.Commands;
//
// namespace TebexUnturned.Legacy
// {
//     public class CommandTebexForcecheck : ITebexCommand
//     {
//         public AllowedCaller AllowedCaller => AllowedCaller.Console;
//
//         public bool RunFromConsole => true;
//
//         public string Name => "tebex:forcecheck";
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
//             TebexLegacy.logWarning("Checking for commands to be executed...");
//             try
//             {               
//                 TebexApiClient wc = new TebexApiClient();
//                 wc.setPlugin(TebexLegacy.Instance);
//                 wc.DoGet("queue", this);
//                 wc.Dispose();
//             }
//             catch (TimeoutException)
//             {
//                 TebexLegacy.logWarning("Timeout!");
//             }
//         }
//
//         public void HandleResponse(JObject response)
//         {
//             if ((int) response["meta"]["next_check"] > 0)
//             {
//                 TebexLegacy.Instance.nextCheck = (int) response["meta"]["next_check"];
//             }
//             
//             if ((bool) response["meta"]["execute_offline"])
//             {
//                 try
//                 {
//                     TebexCommandRunner.doOfflineCommands();
//                 }
//                 catch (Exception e)
//                 {
//                     TebexLegacy.logError(e.ToString());
//                 }
//             }
//             
//             JArray players = (JArray) response["players"];
//
//             foreach (var player in players)
//             {
//                 try
//                 {
//                     CSteamID steamId = new CSteamID((ulong) player["uuid"]);
//                     UnturnedPlayer targetPlayer = UnturnedPlayer.FromCSteamID(steamId);
//
//                     if (targetPlayer.Player != null)
//                     {
//                         TebexLegacy.logWarning("Execute commands for " + (string) targetPlayer.CharacterName + "(ID: "+targetPlayer.CSteamID.ToString()+")");
//                         TebexCommandRunner.doOnlineCommands((int) player["id"], (string) targetPlayer.CharacterName,
//                             targetPlayer.CSteamID.ToString());
//                     }
//                 }
//                 catch (Exception e)
//                 {
//                     TebexLegacy.logError(e.Message);
//                 }
//             }
//         }
//
//         public void HandleError(Exception e)
//         {
//             TebexLegacy.logError("We are unable to fetch your server queue. Please check your secret key.");
//             TebexLegacy.logError(e.ToString());
//         }         
//     }
// }