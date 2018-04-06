using Rocket.API;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;

namespace TebexUnturned
{
    public class ChatListener
    {
        public void Register(IRocketPlugin plugin)
        {
            UnturnedPlayerEvents.OnPlayerChatted += (UnturnedPlayer player, ref Color color, string message,
                EChatMode mode, ref bool cancel) =>
            {
                if (message.Trim() == Tebex.Instance.Configuration.Instance.BuyCommand && Tebex.Instance.Configuration.Instance.BuyEnabled == true)
                {
                    Tebex.logWarning("Message received:" + message.Trim());
                    
                    player.Player.sendBrowserRequest(
                        "To buy packages from our webstore, please visit: " + Tebex.Instance.information.domain,
                        Tebex.Instance.information.domain);                    
                }
            };
        }   
    }
}