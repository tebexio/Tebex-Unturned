using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tebex.Triage
{
    public class TebexTriage
    {
        // For automatically reported errors and issues
        public class AutoTriageEvent
        {
            [JsonProperty("game_id")] public string GameId { get; set; }
            [JsonProperty("framework_id")] public string FrameworkId { get; set; }
            [JsonProperty("plugin_version")] public string PluginVersion { get; set; }
            [JsonProperty("server_ip")] public string ServerIp { get; set; }
            [JsonProperty("error_message")] public string ErrorMessage { get; set; }
            [JsonProperty("trace")] public string Trace { get; set; }
            [JsonProperty("metadata")] public Dictionary<string, string> Metadata { get; set; }
            [JsonProperty("store_name")] public string StoreName { get; set; }
            [JsonProperty("store_url")] public string StoreUrl { get; set; }
        }

        public static AutoTriageEvent CreateAutoTriageEvent(string message, Dictionary<string, string> metadata)
        {
            var triageEvent = new TebexTriage.ReportedTriageEvent();

            triageEvent.ErrorMessage = message;
            triageEvent.Trace = "";
            triageEvent.Metadata = metadata;
            triageEvent.Log = "";

            return triageEvent;
        }
        
        // For issues reported using the /report command
        public class ReportedTriageEvent : AutoTriageEvent
        {
            [JsonProperty("username")] public string Username { get; set; }
            [JsonProperty("user_ip")] public string UserIp { get; set; }
            [JsonProperty("log")] public string Log { get; set; }
        }
    }   
}