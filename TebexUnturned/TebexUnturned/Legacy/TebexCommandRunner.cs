using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Utils;
using SDG.Unturned;

namespace TebexUnturned.Legacy
{
    public class TebexCommandRunner
    {

        public static int deleteAfter = 3;
        
        public static void doOfflineCommands()
        {
            TebexApiClient wc = new TebexApiClient();
            wc.setPlugin(TebexLegacy.Instance);
            wc.Headers.Add("X-Buycraft-Secret", TebexLegacy.Instance.Configuration.Instance.secret);            
            String url = TebexLegacy.Instance.Configuration.Instance.baseUrl + "queue/offline-commands";
            TebexLegacy.logWarning("GET " + url);

            wc.DownloadStringCompleted += (sender, e) =>
            {
                JObject json = JObject.Parse(e.Result);
                JArray commands = (JArray) json["commands"];

                int exCount = 0;
                List<int> executedCommands = new List<int>();
                
                foreach (var command in commands.Children())
                {

                    String commandToRun = buildCommand((string) command["command"], (string) command["player"]["name"],
                        (string) command["player"]["uuid"]);
                    
                    TebexLegacy.logWarning("Run command " + commandToRun);
                    ConsolePlayer executer = new ConsolePlayer(); 
                    TaskDispatcher.QueueOnMainThread(() => {
                    R.Commands.Execute(executer, commandToRun);
                    });
                    executedCommands.Add((int) command["id"]);


                    exCount++;

                    if (exCount % deleteAfter == 0)
                    {
                        try
                        {
                            deleteCommands(executedCommands);
                            executedCommands.Clear();
                        }
                        catch (Exception ex)
                        {
                            TebexLegacy.logError(ex.ToString());
                        }
                    }
                    
                }
                
                TebexLegacy.logWarning(exCount.ToString() + " offline commands executed");
                if (exCount % deleteAfter != 0)
                {
                    try
                    {
                        deleteCommands(executedCommands);
                        executedCommands.Clear();
                    }
                    catch (Exception ex)
                    {
                        TebexLegacy.logError(ex.ToString());
                    }
                }

                wc.Dispose();
            };

            wc.DownloadStringAsync(new Uri(url));
        }

        public static void doOnlineCommands(int playerPluginId, string playerName, string playerId)
        {
            
            TebexLegacy.logWarning("Running online commands for "+playerName+" (" + playerId + ")");
            
            TebexApiClient wc = new TebexApiClient();
            wc.setPlugin(TebexLegacy.Instance);
            wc.Headers.Add("X-Buycraft-Secret", TebexLegacy.Instance.Configuration.Instance.secret);
            String url = TebexLegacy.Instance.Configuration.Instance.baseUrl + "queue/online-commands/" +
                         playerPluginId.ToString();

            TebexLegacy.logWarning("GET " + url);

            wc.DownloadStringCompleted += (sender, e) =>
            {
                JObject json = JObject.Parse(e.Result);
                JArray commands = (JArray) json["commands"];

                int exCount = 0;
                List<int> executedCommands = new List<int>();
                
                foreach (var command in commands.Children())
                {

                    String commandToRun = buildCommand((string) command["command"], playerName, playerId);
                    
                    TebexLegacy.logWarning("Run command " + commandToRun);
                    ConsolePlayer executer = new ConsolePlayer();
                    TaskDispatcher.QueueOnMainThread(() => { R.Commands.Execute(executer, commandToRun); });
                    executedCommands.Add((int) command["id"]);

                    exCount++;

                    if (exCount % deleteAfter == 0)
                    {
                        try
                        {
                            deleteCommands(executedCommands);
                            executedCommands.Clear();
                        }
                        catch (Exception ex)
                        {
                            TebexLegacy.logError(ex.ToString());
                        }
                    }
                    
                }
                
                TebexLegacy.logWarning(exCount.ToString() + " online commands executed for " + playerName);
                if (exCount % deleteAfter != 0)
                {
                    try
                    {
                        deleteCommands(executedCommands);
                        executedCommands.Clear();
                    }
                    catch (Exception ex)
                    {
                        TebexLegacy.logError(ex.ToString());
                    }
                }

                wc.Dispose();
            };

            wc.DownloadStringAsync(new Uri(url));            
        }

        public static void deleteCommands(List<int> commandIds)
        {

            String url = TebexLegacy.Instance.Configuration.Instance.baseUrl + "queue?";
            String amp = "";

            foreach (int CommandId in commandIds)
            {
                url = url + amp + "ids[]=" + CommandId;
                amp = "&";
            }

            TebexLegacy.logWarning("DELETE " + url);

            var request = WebRequest.Create(url);
            request.Method = "DELETE";
            request.Headers.Add("X-Buycraft-Secret", TebexLegacy.Instance.Configuration.Instance.secret);
            
            Thread thread = new Thread(() => request.GetResponse());  
            thread.Start();
        }

        public static string buildCommand(string command, string username, string id)
        {
            return command.Replace("{id}", id).Replace("{username}", username);
        }
    }
}