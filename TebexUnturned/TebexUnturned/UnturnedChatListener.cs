using Rocket.API;
using Rocket.Core.Utils;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tebex.Adapters;
using Tebex.API;
using UnityEngine;

namespace TebexUnturned
{
    public class UnturnedChatListener
    {
        private TebexUnturnedAdapter _adapter;
        private IRocketPlugin _plugin;
        
        public void Register(IRocketPlugin plugin, TebexUnturnedAdapter adapter)
        {
            this._plugin = plugin;
            this._adapter = adapter;
            
            TaskDispatcher.QueueOnMainThread(() => {
            UnturnedPlayerEvents.OnPlayerChatted += (UnturnedPlayer player, ref Color color, string message,
                EChatMode mode, ref bool cancel) =>
            {
                // Check for a configured buy command
                if (message == BaseTebexAdapter.PluginConfig.CustomBuyCommand && BaseTebexAdapter.PluginConfig.BuyEnabled)
                {
                    _adapter.LogInfo($"Buy command received from 'steam:{player.SteamName}/ign:{player.DisplayName}': {message.Trim()}");
                    if (BaseTebexAdapter.Cache.Instance.HasValid("information"))
                    {
                        TebexApi.TebexStoreInfo storeInfo = (TebexApi.TebexStoreInfo)BaseTebexAdapter.Cache.Instance.Get("information").Value;
                    
                        player.Player.sendBrowserRequest(
                            "To buy packages from our webstore, please visit: " + storeInfo.AccountInfo.Domain,
                            storeInfo.AccountInfo.Domain);      
                    }
                    else
                    {
                        _adapter.LogError("Tebex is not setup.");
                        _adapter.ReplyPlayer(player, "This store is not yet setup.");
                    }
                }
            };
            });
        }
    }
}