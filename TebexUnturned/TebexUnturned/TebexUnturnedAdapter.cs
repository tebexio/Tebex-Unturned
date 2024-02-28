using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tebex.API;
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
            throw new System.NotImplementedException();
        }

        public override object GetPlayerRef(string playerId)
        {
            throw new System.NotImplementedException();
        }

        public override string ExpandUsernameVariables(string input, object playerObj)
        {
            throw new System.NotImplementedException();
        }

        public override string ExpandOfflineVariables(string input, TebexApi.PlayerInfo info)
        {
            throw new System.NotImplementedException();
        }

        public override void MakeWebRequest(string endpoint, string body, TebexApi.HttpVerb verb, TebexApi.ApiSuccessCallback onSuccess,
            TebexApi.ApiErrorCallback onApiError, TebexApi.ServerErrorCallback onServerError)
        {
            throw new System.NotImplementedException();
        }

        public override TebexTriage.AutoTriageEvent FillAutoTriageParameters(TebexTriage.AutoTriageEvent partialEvent)
        {
            throw new System.NotImplementedException();
        }
    }
}