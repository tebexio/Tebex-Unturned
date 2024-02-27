// using System;
// using System.Collections.Generic;
// using Newtonsoft.Json.Linq;
// using Rocket.API;
//
// namespace TebexUnturned.Legacy
// {
// public class CommandTebexInfo : ITebexCommand
//     {
//         public AllowedCaller AllowedCaller => AllowedCaller.Console;
//
//         public bool RunFromConsole => true;
//
//         public string Name => "tebex:info";
//
//         public string Help => "Get basic details about the webstore";
//         
//         public string Syntax => "";
//
//         public List<string> Aliases => new List<string>();
//
//         public List<string> Permissions => new List<string>() { "tebex.admin" };
//
//         public void Execute(IRocketPlayer caller, string[] command)
//         {           
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
//             TebexLegacy.Instance.information.id = (int) response["account"]["id"];
//             TebexLegacy.Instance.information.domain = (string) response["account"]["domain"];
//             TebexLegacy.Instance.information.gameType = (string) response["account"]["game_type"];
//             TebexLegacy.Instance.information.name = (string) response["account"]["name"];
//             TebexLegacy.Instance.information.currency = (string) response["account"]["currency"]["iso_4217"];
//             TebexLegacy.Instance.information.currencySymbol = (string) response["account"]["currency"]["symbol"];
//             TebexLegacy.Instance.information.serverId = (int) response["server"]["id"];
//             TebexLegacy.Instance.information.serverName = (string) response["server"]["name"];
//             
//             TebexLegacy.logWarning("Server Information");
//             TebexLegacy.logWarning("=================");
//             TebexLegacy.logWarning("Server "+TebexLegacy.Instance.information.serverName+" for webstore "+TebexLegacy.Instance.information.name+"");
//             TebexLegacy.logWarning("Server prices are in "+TebexLegacy.Instance.information.currency+"");
//             TebexLegacy.logWarning("Webstore domain: "+TebexLegacy.Instance.information.domain+"");
//         }
//
//         public void HandleError(Exception e)
//         {
//             TebexLegacy.logError("We are unable to fetch your server details. Please check your secret key.");
//         }      
//     }
// }