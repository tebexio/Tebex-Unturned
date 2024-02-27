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
//         private void TebexUnbanCommand(IPlayer commandRunner, string command, string[] args)
//         {
//             if (!commandRunner.IsAdmin)
//             {
//                 _adapter.ReplyPlayer(commandRunner, $"{command} can only be used by administrators.");
//                 return;
//             }
//
//             _adapter.ReplyPlayer(commandRunner, $"You must unban players via your webstore.");
//         }
//     }
// }