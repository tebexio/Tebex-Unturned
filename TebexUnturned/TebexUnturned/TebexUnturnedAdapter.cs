using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Commands;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Extensions;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tebex.API;
using Tebex.Triage;
using Logger = Rocket.Core.Logging.Logger;
using Steamworks;
using Tebex.Shared.Components;

namespace Tebex.Adapters
{
    public class TebexUnturnedAdapter : BaseTebexAdapter
    {
        public static Tebex.Plugins.TebexUnturned Plugin { get; private set; }
        public TebexUnturnedAdapter(Tebex.Plugins.TebexUnturned plugin)
        {
            Plugin = plugin;
        }
        
        public override void Init()
        {
            // Initialize timers, hooks, etc. here
            Plugin.PluginTimers().Every(121.0f, () =>
            {
                if (IsTebexReady())
                {
                    ProcessCommandQueue(false);    
                }
            });
            Plugin.PluginTimers().Every(61.0f, () =>
            {
                if (IsTebexReady())
                {
                    DeleteExecutedCommands(false);    
                }
            });
            Plugin.PluginTimers().Every(61.0f, () =>
            {
                if (IsTebexReady())
                {
                    ProcessJoinQueue(false);    
                }
            });
            Plugin.PluginTimers().Every((60.0f * 15) + 1.0f, () =>  // Every 15 minutes for store info
            {
                if (IsTebexReady())
                {
                    RefreshStoreInformation(false);    
                }
            });
            Plugin.PluginTimers().Every(0.5f, () =>
            {
                Task task = Plugin.WebRequests().ProcessNextRequestAsync();
                task.RunSynchronously();
            });

            Provider.onServerConnected += id =>
            {
                var player = GetPlayerRef(id.ToString());
                if (player is SteamPlayer)
                {
                    Plugin.OnUserConnected((player as SteamPlayer).ToUnturnedPlayer());
                }
            };
        }

        public override void SaveConfig(TebexConfig config)
        {
            Plugin.SaveConfiguration();
        }

        public override void LogWarning(string message, string solution)
        {
            Logger.LogWarning(message);
            Logger.LogWarning(solution);
            
            if (PluginConfig.AutoReportingEnabled)
            {
                new PluginEvent(Plugin, Plugin.GetPlatform(), EnumEventLevel.WARNING, message).Send(this);
            }
        }

        public override void LogWarning(string message, string solution, Dictionary<string, string> metadata)
        {
            Logger.LogWarning(message);
            Logger.LogWarning(solution);
            
            if (PluginConfig.AutoReportingEnabled)
            {
                new PluginEvent(Plugin, Plugin.GetPlatform(), EnumEventLevel.ERROR, message).Send(this);
            }
        }

        public override void LogError(string message)
        {
            Logger.LogError(message);
            
            if (PluginConfig.AutoReportingEnabled)
            {
                new PluginEvent(Plugin, Plugin.GetPlatform(), EnumEventLevel.ERROR, message).Send(this);
            }
        }

        public override void LogError(string message, Dictionary<string, string> metadata)
        {
            Logger.LogError(message);
            if (PluginConfig.AutoReportingEnabled)
            {
                new PluginEvent(Plugin, Plugin.GetPlatform(), EnumEventLevel.ERROR, message).Send(this);
            }
        }

        public override void LogInfo(string message)
        {
            Logger.Log(message);
        }

        public override void LogDebug(string message)
        {
            if (PluginConfig.DebugMode)
            {
                Logger.Log("[DEBUG] " + message);   
            }
        }

        public void ReplyPlayer(object player, string message)
        {
            if (player is UnturnedPlayer)
            {
                UnturnedPlayer unturnedPlayer = (UnturnedPlayer)player;
                UnturnedChat.Say(unturnedPlayer, message);
            } else if (player is ConsolePlayer)
            {
                ConsolePlayer consolePlayer = (ConsolePlayer)player;
                UnturnedChat.Say(consolePlayer, message);
            }
            else
            {
                LogError("Cannot send chat message to player of type: " + player.GetType());   
            }
        }

        public override bool ExecuteOfflineCommand(TebexApi.Command command, string commandName, string[] args)
        {
            var serverCommandTaskCompletionSource = new TaskCompletionSource<bool>();
            
            //playerObj is always null for offline commands
            var fullCommand = $"{commandName} {string.Join(" ", args)}";
            //UnturnedPlayer player = (playerObj as SteamPlayer).ToUnturnedPlayer();
            ConsolePlayer executer = new ConsolePlayer();
            TaskDispatcher.QueueOnMainThread(() =>
            {
                if (command.Conditions.Delay > 0)
                {
                    Thread.Sleep(command.Conditions.Delay * 1000);
                }
                
                // Rocket seems to return success only if the command is found, but not necessarily if it successfully executed.
                bool success = R.Commands.Execute(executer, fullCommand);
                if (success)
                {
                    serverCommandTaskCompletionSource.SetResult(true);
                }
                else
                {
                    LogError($"offline command did not succeed for player '{command.Player.Username}': {fullCommand}");
                    serverCommandTaskCompletionSource.SetResult(false);
                }
            });
            serverCommandTaskCompletionSource.Task.Wait();
            return serverCommandTaskCompletionSource.Task.Result;
        }

        private bool ExecuteServerCommand(TebexApi.Command command, TebexApi.DuePlayer duePlayer, string commandName, string[] args)
        {
            var serverCommandTaskCompletionSource = new TaskCompletionSource<bool>();
            var fullCommand = $"{commandName} {string.Join(" ", args)}";


            UnturnedPlayer player = GetPlayerRef(duePlayer.UUID) as UnturnedPlayer;
            ConsolePlayer executor = new ConsolePlayer();
            var commandFound = false;
            TaskDispatcher.QueueOnMainThread(() =>
            {
                // Rocket seems to return success only if the command is found, but not necessarily if it successfully executed.
                commandFound = R.Commands.Execute(executor, fullCommand);
                if (commandFound)
                {
                    serverCommandTaskCompletionSource.SetResult(true);
                }
                else
                {
                    serverCommandTaskCompletionSource.SetResult(false);
                    LogError($"online command did not succeed for player '{player.SteamPlayer().player.name}': {fullCommand}");
                }
            });
            serverCommandTaskCompletionSource.Task.Wait();
            return serverCommandTaskCompletionSource.Task.Result;
        }
        
        public override bool ExecuteOnlineCommand(TebexApi.Command command, TebexApi.DuePlayer duePlayer, string commandName, string[] args)
        {
            return ExecuteServerCommand(command, duePlayer, commandName, args);
        }

        public override bool IsPlayerOnline(TebexApi.DuePlayer duePlayer)
        {
            object player = GetPlayerRef(duePlayer.UUID);
            if (player is UnturnedPlayer)
            {
                return true;
            }

            if (player is SteamPlayer)
            {
                return true;
            }

            if (player == null)
            {
                return false;
            }
            
            LogError("cannot get online status of player type: " + player.GetType());
            return false;
        }

        public override object GetPlayerRef(string playerId)
        {
            // Always returns the steam player if found. playerId provided must be a Steam ID.
            foreach (SteamPlayer player in Provider.clients)
            {
                LogDebug($"provider client: {player.playerID.steamID}/{player.playerID.playerName}");
                if (player.playerID.steamID.m_SteamID.ToString().Equals(playerId))
                {
                    return player;
                }
            }

            return null;
        }

        public override string ExpandUsernameVariables(string input, TebexApi.DuePlayer player)
        {
            SteamPlayer steamPlayer = GetPlayerRef(player.UUID) as SteamPlayer;
            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);

            input = input.Replace("{id}", player.UUID);
            input = input.Replace("{name}", unturnedPlayer.SteamName);
            input = input.Replace("{username}", steamPlayer.playerID.playerName);
            input = input.Replace("{steamname}", unturnedPlayer.SteamName);
            input = input.Replace("{charactername}", unturnedPlayer.CharacterName);
            input = input.Replace("{displayname}", unturnedPlayer.DisplayName);
            input = input.Replace("{uuid}", player.UUID);

            return input;
        }

        public override string ExpandOfflineVariables(string input, TebexApi.PlayerInfo info)
        {
            input = input.Replace("{id}", info.Uuid);
            input = input.Replace("{name}", info.Username);
            input = input.Replace("{username}", info.Username);
            input = input.Replace("{steamname}", info.Username);
            input = input.Replace("{uuid}", info.Uuid);

            return input;
        }

        public override void MakeWebRequest(string endpoint, string body, TebexApi.HttpVerb verb, TebexApi.ApiSuccessCallback onSuccess,
            TebexApi.ApiErrorCallback onApiError, TebexApi.ServerErrorCallback onServerError)
        {
            var headers = new Dictionary<string, string>();
            headers.Add("X-Tebex-Secret", PluginConfig.SecretKey);
            headers.Add("User-Agent", "Unturned/" + Plugins.TebexUnturned.GetPluginVersion());
            Plugin.WebRequests().Enqueue(endpoint, body, (code, response) =>
            {
                if (code == 200 || code == 201 || code == 202 || code == 204)
                {
                    onSuccess?.Invoke(code, response);
                }
                else if (code == 403) // secret key is incorrect or not set
                {
                    if (IsTebexReady()) // user has set the secret key but api is denying access
                    {
                        LogError($"403 Forbidden {endpoint} | {body} -> {response}", new Dictionary<string, string>()
                        {
                            {"body", body},
                            {"url", endpoint},
                            {"code", code.ToString()},
                            {"response", response}
                        });
                    }
                    else
                    {
                        LogWarning("Your server's secret key is not set or incorrect.", "Use tebex:secret \"<key>\" to set your secret key to the one associated with your webstore.");
                    }
                }
                else if (code == 429) // rate limited
                {
                    LogWarning("We are being rate limited by Tebex API. If this issue continues, please report a problem.", "Requests will resume after 5 minutes.");
                    Plugin.PluginTimers().Once(60 * 5, () =>
                    {
                        LogInfo("Tebex rate limit timer has elapsed, processing will now continue");
                        IsRateLimited = false;
                    });
                }
                else if (code == 500)
                {
                    LogError("Internal server error from Plugin API",
                        new Dictionary<string, string>
                        {
                            { "request", body },
                            { "response", response },
                        });
                    LogDebug(
                        "Internal Server Error from Tebex API. Please try again later. Error details follow below.");
                    LogDebug(response);
                    onServerError?.Invoke(code, response);
                }
                else if (code == 530) // cloudflare origin error
                {
                    LogDebug("CDN reported error code, web request not completed: " + code);
                    LogDebug(response);
                    onServerError?.Invoke(code, response);
                }
                else if (code == 0) // timeout or cancelled
                {
                    LogError("Request timeout to Plugin API",
                        new Dictionary<string, string>
                        {
                            { "request", body },
                            { "response", response },
                        });
                    LogDebug("Request Timeout from Tebex API. Please try again later.");
                }
                else // response is a general failure error message in a json formatted response from the api
                {
                    try
                    {
                        var error = JsonConvert.DeserializeObject<TebexApi.TebexError>(response);
                        if (error != null)
                        {
                            LogError(
                                "Plugin API reported general failure", new Dictionary<string, string>
                                {
                                    { "request", body },
                                    { "error", error.ErrorMessage },
                                });
                            onApiError?.Invoke(error);
                        }
                        else
                        {
                            LogError("Plugin API error could not be interpreted!", new Dictionary<string, string>
                                {
                                    { "request", body },
                                    { "response", response },
                                });
                            LogDebug($"Failed to unmarshal an expected error response from API.");
                            onServerError?.Invoke(code, response);
                        }

                        LogDebug($"Request to {endpoint} failed with code {code}.");
                        LogDebug(response);
                    }
                    catch (Exception e) // something really unexpected with our response, it's likely not JSON
                    {
                        // Try to allow server error callbacks to be processed, but they mmay assume the body contains
                        // parseable json when it doesn't.
                        try
                        {
                            LogError($"an unexpected server error occurred: {e.Message}");
                            onServerError?.Invoke(code, response);
                        }
                        catch (JsonReaderException ex)
                        {
                            LogError($"could not parse response from remote as JSON: {ex.Message}");
                            LogError(ex.ToString());
                        }
                    }
                }
            }, verb, headers, 10.0f);
        }

        public void SaveConfiguration()
        {
            Plugin.SaveConfiguration();
        }
    }
}