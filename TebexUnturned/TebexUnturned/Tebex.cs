using System;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Steamworks;

namespace TebexUnturned
{
    public class Tebex : RocketPlugin<TebexConfiguration>
    {
        private DateTime lastCalled = DateTime.Now;
        public static Tebex Instance;

        private void checkCheck()
        {
            if ((DateTime.Now - this.lastCalled).TotalSeconds > 120)
            {
                DoCheck();
                this.lastCalled = DateTime.Now;
            }
        }

        protected override void Load()
        {
            Instance = this;
            Logger.LogWarning("Tebex Loaded");
            if (Instance.Configuration.Instance.secret == "")
            {
                Logger.LogError("You have not yet defined your secret key. Use /tebex secret <secret> to define your key");
            }
        }

        public static void SendChat()
        {
            UnturnedChat.Say("Hello!");
            UnturnedConsole.print("We said hello...");
        }

        public static void SetSecret(String secret)
        {
            
        }

        public static void DoCheck()
        {
        }

        public static void UpdatePackages()
        {
            
        }

        public static void GetInfo()
        {
            
        }


        public void FixedUpdate()
        {
            if (this.isActiveAndEnabled)
                this.checkCheck();
        }
    }       

}