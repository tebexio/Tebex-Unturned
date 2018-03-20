using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
                    Tebex.logWarning("Run command" + (string) command["command"]);
                    CommandWindow.input.onInputText((string) command["command"]);
                    executedCommands.Add((int) command["id"]);

                    exCount++;

                    if (exCount % 20 == 0)
                    {
                        deleteCommands(executedCommands);
                        executedCommands.Clear();
                    }
                    
                }
                
                deleteCommands(executedCommands);
                Tebex.logWarning(exCount.ToString() + " offline commands executed");
                wc.Dispose();
            };

            wc.DownloadStringAsync(new Uri(url));
        }

        public static void deleteCommands(List<int> commandIds)
        {
            TebexApiClient wc = new TebexApiClient();
            wc.setPlugin(Tebex.Instance);
            wc.Headers.Add("X-Buycraft-Secret", Tebex.Instance.Configuration.Instance.secret);            
            String url = Tebex.Instance.Configuration.Instance.baseUrl + "queue?";            
            String amp = "";
            
            foreach (int CommandId in commandIds)
            {
                url = url + amp + "ids[]=" + CommandId;
                amp = "&";
            }
            
            Tebex.logWarning("GET " + url);

            wc.UploadStringCompleted += (sender, e) =>
            {
                wc.Dispose();
            };

            wc.UploadStringAsync(new Uri(url), "DELETE");
        }
    }
}