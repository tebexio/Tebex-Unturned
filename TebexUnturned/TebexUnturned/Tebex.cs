using System;
using System.ComponentModel.Design;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using TebexUnturned.Models;

namespace TebexUnturned
{
    public class Tebex : RocketPlugin<TebexConfiguration>
    {
        private DateTime lastCalled = DateTime.Now;
        public static Tebex Instance;
        public WebstoreInfo information;

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
            this.information = new WebstoreInfo();
            Instance = this;
            
            
            logWarning("Tebex Loaded");
            if (Instance.Configuration.Instance.secret == "")
            {
                logError("You have not yet defined your secret key. Use /tebex:secret <secret> to define your key");
            }
            else
            {
                CommandTebexInfo infoCommand = new CommandTebexInfo();
                String[] command = new[] { "tebex:info" };
                infoCommand.Execute(new ConsolePlayer(), command);
            }

            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, errors) => { return true; };
        }

        public static void SendChat(String message)
        {
            UnturnedChat.Say(message);
            logWarning("We said hello!");
        }

        public static void DoCheck()
        {
        }

        public static void UpdatePackages()
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