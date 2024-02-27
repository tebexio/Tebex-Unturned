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
//         private void TebexReportCommand(IPlayer player, string command, string[] args)
//         {
//             if (!player.HasPermission(command))
//             {
//                 _adapter.ReplyPlayer(player, "You do not have permission to run that command.");
//                 return;
//             }
//
//             if (args.Length == 0) // require /confirm to send
//             {
//                 _adapter.ReplyPlayer(player,
//                     "Please run `tebex.report confirm 'Your description here'` to submit your report. The following information will be sent to Tebex: ");
//                 _adapter.ReplyPlayer(player, "- Your game version, store id, and server IP.");
//                 _adapter.ReplyPlayer(player, "- Your username and IP address.");
//                 _adapter.ReplyPlayer(player, "- Please include a short description of the issue you were facing.");
//             }
//
//             if (args.Length == 2 && args[0] == "confirm")
//             {
//                 _adapter.ReplyPlayer(player, "Sending your report to Tebex...");
//
//                 var triageEvent = new TebexTriage.ReportedTriageEvent();
//                 triageEvent.GameId = $"{game} {server.Version}|{server.Protocol}";
//                 triageEvent.FrameworkId = "Oxide";
//                 triageEvent.PluginVersion = GetPluginVersion();
//                 triageEvent.ServerIp = server.Address.ToString();
//                 triageEvent.ErrorMessage = "Player Report: " + args[1];
//                 triageEvent.Trace = "";
//                 triageEvent.Metadata = new Dictionary<string, string>()
//                 {
//
//                 };
//                 triageEvent.Username = player.Name + "/" + player.Id;
//                 triageEvent.UserIp = player.Address;
//
//                 _adapter.ReportManualTriageEvent(triageEvent,
//                     (code, body) => { _adapter.ReplyPlayer(player, "Your report has been sent. Thank you!"); },
//                     (code, body) =>
//                     {
//                         _adapter.ReplyPlayer(player,
//                             "An error occurred while submitting your report. Please contact our support team directly.");
//                         _adapter.ReplyPlayer(player, "Error: " + body);
//                     });
//
//                 return;
//             }
//
//             _adapter.ReplyPlayer(player, $"Usage: tebex.report <confirm> '<message>'");
//         }
//     }
// }