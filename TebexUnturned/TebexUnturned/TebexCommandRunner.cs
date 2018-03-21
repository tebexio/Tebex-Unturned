using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Design;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SDG.Unturned;

namespace TebexUnturned
{
    public class TebexCommandRunner
    {
        public static void doOfflineCommands()
        {
            TebexApiClient wc = new TebexApiClient();
            wc.setPlugin(Tebex.Instance);
            wc.Headers.Add("X-Buycraft-Secret", Tebex.Instance.Configuration.Instance.secret);            
            String url = Tebex.Instance.Configuration.Instance.baseUrl + "queue/offline-commands";
            Tebex.logWarning("GET " + url);

            wc.DownloadStringCompleted += (sender, e) =>
            {
                JObject json = JObject.Parse(e.Result);
                JArray commands = (JArray) json["commands"];

                int exCount = 0;
                List<int> executedCommands = new List<int>();
                
                foreach (var command in commands.Children())
                {
                    Tebex.logWarning("Run command " + (string) command["command"]);
                    CommandWindow.input.onInputText((string) command["command"]);
                    executedCommands.Add((int) command["id"]);

                    exCount++;

                    if (exCount % 3 == 0)
                    {
                        try
                        {
                            deleteCommands(executedCommands);
                            executedCommands.Clear();
                        }
                        catch (Exception ex)
                        {
                            Tebex.logError(ex.ToString());
                        }
                    }
                    
                }
                
                Tebex.logWarning(exCount.ToString() + " offline commands executed");
                if (exCount % 3 != 0)
                {
                    try
                    {
                        Tebex.logWarning("Delete in separate thread....");
                        deleteCommands(executedCommands);
                        executedCommands.Clear();
                    }
                    catch (Exception ex)
                    {
                        Tebex.logError(ex.ToString());
                    }
                }

                wc.Dispose();
            };

            wc.DownloadStringAsync(new Uri(url));
        }

        public static void doOnlineCommands(int playerPluginId, string playerName, string playerId)
        {
            
        }

        public static void deleteCommands(List<int> commandIds)
        {

            String url = Tebex.Instance.Configuration.Instance.baseUrl + "queue?";
            String amp = "";

            foreach (int CommandId in commandIds)
            {
                url = url + amp + "ids[]=" + CommandId;
                amp = "&";
            }

            Tebex.logWarning("DELETE " + url);

            var request = WebRequest.Create(url);
            request.Method = "DELETE";
            request.Headers.Add("X-Buycraft-Secret", Tebex.Instance.Configuration.Instance.secret);
            
            Tebex.logWarning("Delete in separate thread....");
            Thread thread = new Thread(() => request.GetResponse());  
            thread.Start();
        }
    }
}