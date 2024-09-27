using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;
using Tebex.Adapters;
using Tebex.API;

namespace Tebex.Triage
{
    public enum EnumEventLevel
    {
        INFO,
        WARNING,
        ERROR
    }

    /// <summary>
    /// PluginEvent represents a reportable event that occurred during runtime.
    /// </summary>
    public class PluginEvent
    {
        public static ConcurrentQueue<PluginEvent> PLUGIN_EVENTS = new ConcurrentQueue<PluginEvent>();
        
        // Data attached to all plugin events, set via Init()
        public static string SERVER_IP = "";
        public static string SERVER_ID = "";
        public static string STORE_URL = "";
        public static bool IS_DISABLED = false;

        [JsonProperty("game_id")] private string GameId { get; set; }
        [JsonProperty("framework_id")] private string FrameworkId { get; set; }
        [JsonProperty("runtime_version")] private string RuntimeVersion { get; set; }

        [JsonProperty("framework_version")]
        private string FrameworkVersion { get; set; }

        [JsonProperty("plugin_version")] private string PluginVersion { get; set; }
        [JsonProperty("server_id")] private string ServerId { get; set; }
        [JsonProperty("event_message")] private string EventMessage { get; set; }
        [JsonProperty("event_level")] private String EventLevel { get; set; }
        [JsonProperty("metadata")] private Dictionary<string, string> Metadata { get; set; }
        [JsonProperty("trace")] private string Trace { get; set; }

        [JsonProperty("store_url")] private string StoreUrl { get; set; }
        
        [JsonProperty("server_ip")] private string ServerIp { get; set; }

        [JsonIgnore]
        public TebexPlatform platform;
        
        private Plugins.TebexUnturned _plugin;
        
        public PluginEvent(Plugins.TebexUnturned plugin, TebexPlatform platform, EnumEventLevel level, string message)
        {
            _plugin = plugin;
            this.platform = platform;

            TebexTelemetry tel = platform.GetTelemetry();

            GameId = "Unturned";
            FrameworkId = tel.GetServerSoftware();
            RuntimeVersion = tel.GetRuntimeVersion();
            FrameworkVersion = tel.GetServerVersion();
            PluginVersion = platform.GetPluginVersion();
            EventLevel = level.ToString();
            EventMessage = message;
            Trace = "";
            ServerIp = PluginEvent.SERVER_IP;
            ServerId = PluginEvent.SERVER_ID;
            StoreUrl = PluginEvent.STORE_URL;
        }

        public PluginEvent WithTrace(string trace)
        {
            Trace = trace;
            return this;
        }

        public PluginEvent WithMetadata(Dictionary<string, string> metadata)
        {
            Metadata = metadata;
            return this;
        }

        public void Send(BaseTebexAdapter adapter)
        {
            if (IS_DISABLED)
            {
                return;
            }

            PLUGIN_EVENTS.Enqueue(this);
            if (PLUGIN_EVENTS.Count >= 10)
            {
                SendAllEvents(adapter);
            }
        }

        /// <summary>
        /// SendAllEvents will attempt to send all queued plugin events. If we have too many to send (we receive 413 Request Content Too Large)
        /// then we will batch the events we need to send in <see cref="_trySendTooLargeEvents"/>
        ///
        /// The event queue is cleared as a result of this function
        /// </summary>
        /// <param name="adapter"></param>
        public static void SendAllEvents(BaseTebexAdapter adapter)
        {
            // Dequeue each event into a list that we will serialize.
            List<PluginEvent> eventsToSend = new List<PluginEvent>();
            while (PLUGIN_EVENTS.Count > 0)
            {
                var success = PLUGIN_EVENTS.TryDequeue(out var pluginEvent);
                if (success)
                {
                    eventsToSend.Add(pluginEvent);    
                }
            }
            
            adapter.MakeWebRequest("https://plugin-logs.tebex.io/events", JsonConvert.SerializeObject(eventsToSend), TebexApi.HttpVerb.POST,
                (code, body) =>
                {
                    if (code < 300 && code > 199) // success
                    {
                        adapter.LogDebug("Successfully sent plugin events");
                        return;
                    }
                    else if (code == 413) // Request Entity Too Large can occur with especially large event groups
                    {
                        _trySendTooLargeEvents(adapter);
                        return;
                    }
                    
                    adapter.LogDebug("Failed to send plugin logs. Unexpected response code: " + code);
                    adapter.LogDebug(body);
                }, (pluginLogsApiError) =>
                {
                    adapter.LogDebug("Failed to send plugin logs. Unexpected Tebex API error: " + pluginLogsApiError);
                }, (pluginLogsServerErrorCode, pluginLogsServerErrorResponse) =>
                {
                    adapter.LogDebug("Failed to send plugin logs. Unexpected server error: " + pluginLogsServerErrorResponse);
                });
        }

        /// <summary>
        /// SendSpecificEvents will send the provided list of events to our plugin logs system.
        /// </summary>
        /// <param name="adapter">The adapter sending the events</param>
        /// <param name="events">A list of <see cref="PluginEvent"/>s</param>
        public static void SendSpecificEvents(BaseTebexAdapter adapter, List<PluginEvent> events)
        {
            adapter.MakeWebRequest("https://plugin-logs.tebex.io/events", JsonConvert.SerializeObject(events), TebexApi.HttpVerb.POST,
                (code, body) =>
                {
                    if (code < 300 && code > 199) // success
                    {
                        adapter.LogDebug("Successfully sent batched plugin events");
                        return;
                    }
                    
                    adapter.LogDebug("Failed to send batched plugin events. Unexpected response code: " + code);
                    adapter.LogDebug(body);
                }, (pluginLogsApiError) =>
                {
                    adapter.LogDebug("Failed to send batched plugin events. Unexpected Tebex API error: " + pluginLogsApiError);
                }, (pluginLogsServerErrorCode, pluginLogsServerErrorResponse) =>
                {
                    adapter.LogDebug("Failed to send batched plugin events. Unexpected server error: " + pluginLogsServerErrorResponse);
                });
        }
        
        /// <summary>
        /// _trySendTooLargeEvents handles situations where we have too many events to send to Tebex leading to a 413 Request Content Too Large error.
        /// This batches events in groups of 5 until the events queue is empty.
        /// </summary>
        /// <param name="adapter">The adapter sending the events</param>
        private static void _trySendTooLargeEvents(BaseTebexAdapter adapter)
        {
            List<PluginEvent> eventsBatch = new List<PluginEvent>();
            while (PLUGIN_EVENTS.Count > 0)
            {
                var dequeueSuccess = PLUGIN_EVENTS.TryDequeue(out PluginEvent eventToSend);
                if (!dequeueSuccess || eventToSend == null)
                {
                    adapter.LogDebug("failed to dequeue plugin event");
                    continue;                   
                }
                
                eventsBatch.Add(eventToSend);

                // Batch in groups of 5
                if (eventsBatch.Count >= 5)
                {
                    SendSpecificEvents(adapter, eventsBatch);
                    eventsBatch.Clear();
                }
            }

            // Any events still left in the list, send them
            if (eventsBatch.Count > 0)
            {
                SendSpecificEvents(adapter, eventsBatch);
            }
        }
    }
}