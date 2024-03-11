using System.Collections.Generic;
using System.Net;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tebex.Shared.Components;
using Tebex.Triage;

namespace TebexUnturned.Commands
{
    public class ReportCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public bool RunFromConsole => true;

        public string Name => "tebex:report";

        public string Help => "Sends a technical issue report to Tebex";
        
        public string Syntax => "tebex:report confirm <message>";

        public List<string> Aliases => new List<string>() { "tebex.report" };

        public List<string> Permissions => new List<string>() { "tebex.admin" };

        public void Execute(IRocketPlayer commandRunner, string[] args)
        {
            var _adapter = Tebex.Plugins.TebexUnturned.GetAdapter();
            if (!_adapter.IsReady)
            {
                _adapter.ReplyPlayer(commandRunner, "Tebex is not setup.");
            }

            if (!commandRunner.HasPermission(Permissions[0]))
            {
                _adapter.ReplyPlayer(commandRunner, "You do not have permission to run that command.");
                return;
            }

            if (args.Length == 0) // require /confirm to send
            {
                _adapter.ReplyPlayer(commandRunner,
                    "Please run `tebex.report confirm 'Your description here'` to submit your report. The following information will be sent to Tebex: ");
                _adapter.ReplyPlayer(commandRunner, "- Your game version, store id, and server IP.");
                _adapter.ReplyPlayer(commandRunner, "- Your username and IP address.");
                _adapter.ReplyPlayer(commandRunner, "- Please include a short description of the issue you were facing.");
            }

            if (args.Length == 2 && args[0] == "confirm")
            {
                _adapter.ReplyPlayer(commandRunner, "Sending your report to Tebex...");

                var triageEvent = new TebexTriage.ReportedTriageEvent();
                triageEvent.GameId = $"Unturned";
                triageEvent.FrameworkId = "RocketMod";
                triageEvent.PluginVersion = Tebex.Plugins.TebexUnturned.GetPluginVersion();
                triageEvent.ServerIp = new IPAddress(Provider.ip).ToString();
                triageEvent.ErrorMessage = "Player Report: " + args[1];
                triageEvent.Trace = "";
                triageEvent.Metadata = new Dictionary<string, string>()
                {

                };
                triageEvent.Username = commandRunner.DisplayName + "/" + commandRunner.Id;
                triageEvent.UserIp = commandRunner is UnturnedPlayer ? ((UnturnedPlayer)commandRunner).IP : "0.0.0.0";

                _adapter.ReportManualTriageEvent(triageEvent,
                    (code, body) => { _adapter.ReplyPlayer(commandRunner, "Your report has been sent. Thank you!"); },
                    (code, body) =>
                    {
                        _adapter.ReplyPlayer(commandRunner,
                            "An error occurred while submitting your report. Please contact our support team directly.");
                        _adapter.ReplyPlayer(commandRunner, "Error: " + body);
                    });

                return;
            }

            _adapter.ReplyPlayer(commandRunner, $"Usage: tebex.report <confirm> '<message>'");
        }
    }
}