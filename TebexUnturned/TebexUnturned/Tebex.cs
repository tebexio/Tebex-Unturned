using System;
using System.Json;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;

namespace TebexUnturned
{
    public class Tebex : RocketPlugin<TebexConfiguration>
    {
        private DateTime lastCalled = DateTime.Now;
        public static Tebex Instance;
        public TebexWebclient webclient;

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
            Instance.webclient = new TebexWebclient(Instance);
            
            logWarning("Tebex Loaded");
            if (Instance.Configuration.Instance.secret == "")
            {
                logError("You have not yet defined your secret key. Use /tebex secret <secret> to define your key");
            }
        }

        public static void SendChat()
        {
            UnturnedChat.Say("Hello!");
            UnturnedConsole.print("We said hello...");
            logWarning("We said hello!");
        }

        public static void SetSecret(String secret)
        {
            Instance.Configuration.Instance.secret = secret;
            Instance.webclient.Get("information", value => { });
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

        public static void logWarning(String message)
        {
            Logger.LogWarning(message);
        }

        public static void logError(String message)
        {
            Logger.LogError(message);
        }

    }       

}