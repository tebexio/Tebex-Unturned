// using System;
// using UnityEngine;
// using Rocket.API;
// using Rocket.Unturned.Player;
// using Rocket.Unturned;
// using Rocket.Unturned.Commands;
// using Rocket.Unturned.Chat;
// using System.Collections.Generic;
// using Newtonsoft.Json.Linq;
//
// namespace TebexUnturned.Legacy
// {
//     public class CommandTebexSecret : ITebexCommand
//     {
//         public AllowedCaller AllowedCaller => AllowedCaller.Console;
//
//         public bool RunFromConsole => true;
//
//         public string Name => "tebex:secret";
//
//         public string Help => "Set your server secret";
//         
//         public string Syntax => "<secret>";
//
//         public List<string> Aliases => new List<string>();
//
//         public List<string> Permissions => new List<string>() { "tebex.admin" };
//
//         public void Execute(IRocketPlayer caller, string[] command)
//         {
//             String secret = command[0];
//             TebexLegacy.Instance.Configuration.Instance.secret = secret;
//             TebexLegacy.Instance.Configuration.Save();
//             
//             try
//             {               
//                 TebexApiClient wc = new TebexApiClient();
//                 wc.setPlugin(TebexLegacy.Instance);
//                 wc.DoGet("information", this);
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
//             
//             TebexLegacy.Instance.information.id = (int) response["account"]["id"];
//             TebexLegacy.Instance.information.domain = (string) response["account"]["domain"];
//             TebexLegacy.Instance.information.gameType = (string) response["account"]["game_type"];
//             TebexLegacy.Instance.information.name = (string) response["account"]["name"];
//             TebexLegacy.Instance.information.currency = (string) response["account"]["currency"]["iso_4217"];
//             TebexLegacy.Instance.information.currencySymbol = (string) response["account"]["currency"]["symbol"];
//             TebexLegacy.Instance.information.serverId = (int) response["server"]["id"];
//             TebexLegacy.Instance.information.serverName = (string) response["server"]["name"];
//             
//             TebexLegacy.logWarning("Your secret key has been validated! Webstore Name: " + TebexLegacy.Instance.information.name);
//         }
//
//         public void HandleError(Exception e)
//         {
//             TebexLegacy.logError("We were unable to validate your secret key.");
//         }
//     }
// }