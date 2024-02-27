using Newtonsoft.Json;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Tebex.API;
using Tebex.Triage;

namespace Tebex.Adapters
{
    public class TebexUnturnedAdapter : BaseTebexAdapter
    {
        public static Oxide.Plugins.Tebex Plugin { get; private set; }

        public TebexUnturnedAdapter(Oxide.Plugins.Tebex plugin)
        {
            Plugin = plugin;
        }

        public override void Init()
        {
            // Initialize timers, hooks, etc. here
            
            /*
             * NOTE: We have noticed interesting behavior with plugin timers here in that Rust attempts to "catch up"
             *  on events that it missed instead of skipping ticks in the event of sleep, lag, etc. This caused
             *  hundreds of events to fire simultaneously for our timers. To handle this we will rate limit the plugin's
             *  requests when a 429 is received.
             */
            Plugin.PluginTimers().Every(121.0f, () =>
            {
                ProcessCommandQueue(false);
            });
            Plugin.PluginTimers().Every(61.0f, () =>
            {
                DeleteExecutedCommands(false);
            });
            Plugin.PluginTimers().Every(61.0f, () =>
            {
                ProcessJoinQueue(false);
            });
            Plugin.PluginTimers().Every((60.0f * 15) + 1.0f, () =>  // Every 15 minutes for store info
            {
                RefreshStoreInformation(false);
            });
        }

        public override void LogWarning(string message)
        {
            Plugin.Warn(message);
        }

        public override void LogError(string message)
        {
            Plugin.Error(message);
        }

        public override void LogInfo(string message)
        {
            Plugin.Info(message);
        }

        public override void LogDebug(string message)
        {
            if (PluginConfig.DebugMode)
            {
                Plugin.Error($"[DEBUG] {message}");    
            }
        }

        /**
             * Sends a web request to the Tebex API. This is just a wrapper around webrequest.Enqueue, but passes through
             * multiple callbacks that can be used to interact with each API function based on the response received.
             */
        public override void MakeWebRequest(string endpoint, string body, TebexApi.HttpVerb verb,
            TebexApi.ApiSuccessCallback onSuccess, TebexApi.ApiErrorCallback onApiError,
            TebexApi.ServerErrorCallback onServerError)
        {
            // Use Oxide request method for the webrequests call. We use HttpVerb in the api so as not to depend on
            // Oxide.
            RequestMethod method;
            Enum.TryParse<RequestMethod>(verb.ToString(), true, out method);
            if (method == null)
            {
                LogDebug($"Unknown HTTP method!: {verb.ToString()} {endpoint} | {body}");
                return;
            }

            var headers = new Dictionary<string, string>
            {
                { "X-Tebex-Secret", PluginConfig.SecretKey },
                { "Content-Type", "application/json" }
            };

            var url = endpoint;
            var logOutStr = $"-> {method.ToString()} {url} | {body}";
            
            LogDebug(logOutStr); // Write the full output entry to a debug log
            if (logOutStr.Length > 256) // Limit any sent size of an output string to 256 characters, to prevent sending too much data
            {
                logOutStr = logOutStr.Substring(0, 251) + "[...]";
            }
            
            if (IsRateLimited)
            {
                LogDebug("Skipping web request as rate limiting is enabled.");
                return;
            }
            
            Plugin.WebRequests().Enqueue(url, body, (code, response) =>
            {
                var truncatedResponse = response;
                if (truncatedResponse.Length > 256) // As above limit any data logged or sent to 256 characters
                {
                    truncatedResponse = truncatedResponse.Substring(0, 251) + "[...]";
                }
                
                var logInStr = $"{code} | '{truncatedResponse}' <- {method.ToString()} {url}";
                LogDebug(logInStr);
                
                // To prevent issues where triage events try to be reported due to server issues on the triage API itself,
                //   handle any triage response callbacks here
                if (url.Contains(TebexApi.TebexTriageUrl))
                {
                    try
                    {
                        switch (code)
                        {
                            case 200:
                            case 201:
                            case 202:
                            case 204:
                                onSuccess?.Invoke(code, response);
                                return;
                            case 400:
                                onServerError?.Invoke(code, response);
                                return;
                            case 500:
                                onServerError?.Invoke(code, response);
                                return;
                            default:
                                LogDebug($"Unexpected response code from plugin logs API: {code}");
                                return;
                        }                        
                    }
                    catch (Exception e)
                    {
                        LogDebug($"Failed to handle automatic error log request: {e.Message}");
                        LogDebug(e.ToString());
                        return;
                    }
                }
                
                // We should never have an HTML response passed to callback functions which might assume is JSON
                if (body.Contains("DOCTYPE html") || body.StartsWith("<html"))
                {
                    LogDebug("> Unexpected html response from web request!");
                    return;
                }
                
                if (code == 200 || code == 201 || code == 202 || code == 204)
                {
                    onSuccess?.Invoke(code, response);
                }
                else if (code == 403) // Admins get a secret key warning on any command that's rejected
                {
                    if (url.Contains(TebexApi.TebexApiBase))
                    {
                        LogError("Your server's secret key is either not set or incorrect.");
                        LogError("tebex.secret <key>\" to set your secret key to the one associated with your webstore.");
                        LogError("Set up your store and get your secret key at https://tebex.io/");
                    }
                }
                else if (code == 429) // Rate limited
                {
                    // Rate limits sent from Tebex enforce a 5 minute cooldown.
                    LogWarning("We are being rate limited by Tebex API. If this issue continues, please report a problem.");
                    LogWarning("Requests will resume after 5 minutes.");
                    Plugin.PluginTimers().Once(60 * 5, () =>
                    {
                        LogDebug("Rate limit timer has elapsed.");
                        IsRateLimited = false;
                    });
                }
                else if (code == 500)
                {
                    ReportAutoTriageEvent(TebexTriage.CreateAutoTriageEvent("Internal server error from Plugin API",
                        new Dictionary<string, string>
                        {
                            { "request", logOutStr },
                            { "response", logInStr },
                        }));
                    LogDebug(
                        "Internal Server Error from Tebex API. Please try again later. Error details follow below.");
                    LogDebug(response);
                    onServerError?.Invoke(code, response);
                }
                else if (code == 530) // Cloudflare origin error
                {
                    LogDebug("CDN reported error code, web request not completed: " + code);
                    LogDebug(response);
                    onServerError?.Invoke(code, response);
                }
                else if (code == 0)
                {
                    ReportAutoTriageEvent(TebexTriage.CreateAutoTriageEvent("Request timeout to Plugin API",
                        new Dictionary<string, string>
                        {
                            { "request", logOutStr },
                            { "response", logInStr },
                        }));
                    LogDebug("Request Timeout from Tebex API. Please try again later.");
                }
                else // This should be a general failure error message with a JSON-formatted response from the API.
                {
                    try
                    {
                        var error = JsonConvert.DeserializeObject<TebexApi.TebexError>(response);
                        if (error != null)
                        {
                            ReportAutoTriageEvent(TebexTriage.CreateAutoTriageEvent(
                                "Plugin API reported general failure", new Dictionary<string, string>
                                {
                                    { "request", logOutStr },
                                    { "error", error.ErrorMessage },
                                }));
                            onApiError?.Invoke(error);
                        }
                        else
                        {
                            ReportAutoTriageEvent(TebexTriage.CreateAutoTriageEvent(
                                "Plugin API error could not be interpreted!", new Dictionary<string, string>
                                {
                                    { "request", logOutStr },
                                    { "response", logInStr },
                                }));
                            LogDebug($"Failed to unmarshal an expected error response from API.");
                            onServerError?.Invoke(code, response);
                        }

                        LogDebug($"Request to {url} failed with code {code}.");
                        LogDebug(response);
                    }
                    catch (Exception e) // Something really unexpected with our response and it's likely not JSON
                    {
                        ReportAutoTriageEvent(TebexTriage.CreateAutoTriageEvent(
                        "Did not handle error response from API", new Dictionary<string, string>
                        {
                            { "request", logOutStr },
                            { "response", logInStr },
                        }));
                        
                        LogDebug("Could not gracefully handle error response.");
                        LogDebug($"Response from remote {response}");
                        LogDebug(e.ToString());

                        // Try to allow server error callbacks to be processed, but they may assume the body contains
                        // parseable json when it doesn't.
                        try
                        {
                            onServerError?.Invoke(code, $"{e.Message}: {response}");    
                        }
                        catch (JsonReaderException jsonException)
                        {
                            LogDebug($"Could not parse response from remote as JSON. {jsonException.Message}: {response}");
                        }
                    }
                }
            }, Plugin, method, headers, 10.0f);
        }

        public override void ReplyPlayer(object player, string message)
        {
            var playerInstance = player as IPlayer;
            if (playerInstance != null)
            {
                playerInstance.Reply("{0}", "", message);
            }
        }

        public override void ExecuteOfflineCommand(TebexApi.Command command, object player, string commandName, string[] args)
        {
            if (command.Conditions.Delay > 0)
            {
                // Command requires a delay, use built-in plugin timer to wait until callback
                // in order to respect game threads
                Plugin.PluginTimers().Once(command.Conditions.Delay,
                    () =>
                    {
                        ExecuteServerCommand(command, player as IPlayer, commandName, args);
                    });
            }
            else // No delay, execute immediately
            {
                ExecuteServerCommand(command, player as IPlayer, commandName, args);
            }
        }

        private void ExecuteServerCommand(TebexApi.Command command, IPlayer player, string commandName, string[] args)
        {
            // For the say command, don't pass args or they will all get quoted in chat.
            if (commandName.Equals("chat.add") && args.Length >= 2 && player != null && args[0].ToString().Equals(player.Id))
            {
                var message = string.Join(" ", args.Skip(2));
                
                // Remove leading and trailing quotes if present
                if (message.StartsWith('"'))
                {
                    message = message.Substring(1, message.Length - 1);
                }

                if (message.EndsWith('"'))
                {
                    message = message.Substring(0, message.Length - 1);
                }
                
                player.Message(message);
                return;
            }

            var fullCommand = $"{commandName} {string.Join(" ", args)}";
            Plugin.Server().Command(fullCommand);
        }
        
        public override bool IsPlayerOnline(string playerRefId)
        {
            // Get a reference to the in-game player instance
            IPlayer iPlayer = GetPlayerRef(playerRefId) as IPlayer;
            if (iPlayer == null) // Player is not connected
            {
                return false;
            }

            // IsConnected might indicate just a connection to the server, but unknown if it's possible to have an IPlayer
            // reference and for the player to not actually be connected. This would create a case where we can't
            // check inventory slots prior to package delivery.
            return iPlayer.IsConnected;
        }

        public override object GetPlayerRef(string playerId)
        {
            return Plugin.PlayerManager().FindPlayer(playerId);
        }

        public override bool ExecuteOnlineCommand(TebexApi.Command command, object playerObj, string commandName,
            string[] args)
        {
            try
            {
                if (command.Conditions.Slots > 0)
                {
                    #if RUST
                    // Cast down to the base player in order to get inventory slots available.
                    var player = playerObj as Oxide.Game.Rust.Libraries.Covalence.RustPlayer;
                    BasePlayer basePlayer = player.Object as BasePlayer;
                    var slotsAvailable = basePlayer.inventory.containerMain.capacity - basePlayer.inventory.containerMain.itemList.Count;                    
                    LogDebug($"Detected {slotsAvailable} slots in main inventory where command wants {command.Conditions.Slots}");
                    
                    // Some commands have slot requirements, don't execute those if the player can't accept it
                    if (slotsAvailable < command.Conditions.Slots)
                    {
                        LogWarning($"> Player has command {command.CommandToRun} but not enough main inventory slots. Need {command.Conditions.Slots} empty slots.");
                        return false;
                    }
                    #else
                    LogWarning($"> Command has slots condition, but slots are not supported in this game.");
                    #endif
                }
                
                if (command.Conditions.Delay > 0)
                {
                    // Command requires a delay, use built-in plugin timer to wait until callback
                    // in order to respect game threads
                    Plugin.PluginTimers().Once(command.Conditions.Delay,
                        () =>
                        {
                            ExecuteServerCommand(command, playerObj as IPlayer, commandName, args);
                        });
                }
                else
                {
                    ExecuteServerCommand(command, playerObj as IPlayer, commandName, args);    
                }
            }
            catch (Exception e)
            {
                ReportAutoTriageEvent(TebexTriage.CreateAutoTriageEvent("Caused exception while executing online command", new Dictionary<string, string>()
                {
                    {"command", command.CommandToRun},
                    {"exception", e.Message},
                    {"trace", e.StackTrace},
                }));
                LogError("Failed to run online command due to exception. Command run is aborted.");
                LogError(e.ToString());
                return false;
            }
            
            return true;
        }

        public override string ExpandOfflineVariables(string input, TebexApi.PlayerInfo info)
        {
            string parsed = input;
            parsed = parsed.Replace("{id}", info.Uuid); // In offline commands there is a "UUID" param for the steam ID, and this ID is an internal plugin ID
            parsed = parsed.Replace("{username}", info.Username);
            parsed = parsed.Replace("{name}", info.Username);

            if (parsed.Contains("{") || parsed.Contains("}"))
            {
                LogDebug($"Detected lingering curly braces after expanding offline variables!");
                LogDebug($"Input: {input}");
                LogDebug($"Parsed: {parsed}");
            }

            return parsed;
        }

        public override string ExpandUsernameVariables(string input, object playerObj)
        {
            IPlayer iPlayer = playerObj as IPlayer;
            if (iPlayer == null)
            {
                LogError($"Could not cast player instance when expanding username variables: {playerObj}");
                ReportAutoTriageEvent(TebexTriage.CreateAutoTriageEvent("Could not cast player instance when expanding username variables", new Dictionary<string, string>
                {
                    {"input", input},
                    {"playerObj", playerObj?.ToString()},
                }));
                return input;
            }

            if (input.Contains("{username}") && string.IsNullOrEmpty(iPlayer.Name))
            {
                LogError($"Player name is null while expanding username?!: {iPlayer}");
                LogError($"Base player object: {playerObj}");
                LogError($"Input command: {input}");
                ReportAutoTriageEvent(TebexTriage.CreateAutoTriageEvent("Player ID is null while expanding username?!: ", new Dictionary<string, string>
                {
                    {"input", input},
                    {"iPlayer.Id", iPlayer.Id},
                    {"iPlayer.Name", iPlayer.Name}
                }));
                return input;
            }

            string parsed = input;
            parsed = parsed.Replace("{id}", iPlayer.Id);
            parsed = parsed.Replace("{username}", iPlayer.Name);
            parsed = parsed.Replace("{name}", iPlayer.Name);

            if (parsed.Contains("{") || parsed.Contains("}"))
            {
                LogDebug($"Detected lingering curly braces after expanding username variables!");
                LogDebug($"Input: {input}");
                LogDebug($"Parsed: {parsed}");
            }

            return parsed;
        }

        public override TebexTriage.AutoTriageEvent FillAutoTriageParameters(TebexTriage.AutoTriageEvent partialEvent)
        {
            partialEvent.GameId = $"{Plugin.GetGame()} {Plugin.Server().Version} | {Plugin.Server().Protocol}";
            partialEvent.FrameworkId = "Oxide";
            partialEvent.PluginVersion = Oxide.Plugins.Tebex.GetPluginVersion();
            partialEvent.ServerIp = Plugin.Server().Address.ToString();
            
            return partialEvent;
        }
    }
}