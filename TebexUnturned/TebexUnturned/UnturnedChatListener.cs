using Rocket.API;
using Rocket.Core.Utils;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tebex.Adapters;
using TebexUnturned.Legacy;
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
                if (message == TebexLegacy.Instance.Configuration.Instance.BuyCommand && TebexLegacy.Instance.Configuration.Instance.BuyEnabled)
                {
                    TebexLegacy.logWarning("Message received:" + message.Trim());
                    player.Player.sendBrowserRequest(
                        "To buy packages from our webstore, please visit: " + TebexLegacy.Instance.information.domain,
                        TebexLegacy.Instance.information.domain);                    
                }
            };
            });
        }
    }
}