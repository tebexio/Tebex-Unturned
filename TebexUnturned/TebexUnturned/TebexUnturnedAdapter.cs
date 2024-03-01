using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tebex.API;
using Tebex.Shared.Components;
using Tebex.Triage;
using Logger = Rocket.Core.Logging.Logger;

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
            Logger.LogWarning(message);
        }

        public override void LogError(string message)
        {
            Logger.LogError(message);
        }

        public override void LogInfo(string message)
        {
            Logger.Log(message);
        }

        public override void LogDebug(string message)
        {
            if (PluginConfig.DebugMode)
            {
                Logger.Log("[DEBUG]" + message);   
            }
        }

        public override void ReplyPlayer(object player, string message)
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

        public override void ExecuteOfflineCommand(TebexApi.Command command, object playerObj, string commandName, string[] args)
        {
            throw new System.NotImplementedException();
        }

        public override bool ExecuteOnlineCommand(TebexApi.Command command, object playerObj, string commandName, string[] args)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsPlayerOnline(string playerRefId)
        {
            object player = GetPlayerRef(playerRefId);
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

        public override string ExpandUsernameVariables(string input, object playerObj)
        {
            SteamPlayer steamPlayer = (SteamPlayer)playerObj;
            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);

            input = input.Replace("{id}", steamPlayer.playerID.steamID.ToString());
            input = input.Replace("{name}", unturnedPlayer.SteamName);
            input = input.Replace("{username}", steamPlayer.playerID.playerName);
            input = input.Replace("{steamname}", unturnedPlayer.SteamName);
            input = input.Replace("{charactername}", unturnedPlayer.CharacterName);
            input = input.Replace("{displayname}", unturnedPlayer.DisplayName);
            input = input.Replace("{uuid}", steamPlayer.playerID.steamID.ToString());

            return input;
        }

        public override string ExpandOfflineVariables(string input, TebexApi.PlayerInfo info)
        {
            input = input.Replace("{id}", info.Id);
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
            headers.Add("Content-Type", "application/json");
            headers.Add("X-Tebex-Secret", PluginConfig.SecretKey);
            
            Plugin.WebRequests().Enqueue(endpoint, body, (code, response) =>
            {
                if (code == 200 || code == 201 || code == 202 || code == 204)
                {
                    onSuccess?.Invoke(code, response);
                }
                else if (code == 403)
                {
                    LogError("Your server's secret key is either not set or incorrect.");
                    LogError("Use tebex:secret \"<key>\" to set your secret key to the one associated with your webstore.");
                    LogError("Set up your store and get your secret key at https://tebex.io/");
                }
                else if (code == 429) // rate limited
                {
                    LogWarning("We are being rate limited by Tebex API. If this issue continues, please report a problem.");
                    LogWarning("Requests will resume after 5 minutes.");
                    Plugin.PluginTimers().Once(60 * 5, () =>
                    {
                        LogWarning("Tebex rate limit timer has elapsed, processing will now continue");
                        IsRateLimited = false;
                    });
                }
                else if (code == 500)
                {
                    //TODO triage report
                    onServerError?.Invoke(code, response);
                }
                else if (code == 530) // cloudflare origin error
                {
                    //TODO triage report
                    onServerError?.Invoke(code, response);
                }
                else if (code == 0) // timeout or cancelled
                {
                    //TODO
                }
                else // response is a general failure error message in a json formatted response from the api
                {
                    //TODO on api error
                    
                    try
                    {
                        // TODO
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

        public override TebexTriage.AutoTriageEvent FillAutoTriageParameters(TebexTriage.AutoTriageEvent partialEvent)
        {
            throw new System.NotImplementedException();
        }
    }
}