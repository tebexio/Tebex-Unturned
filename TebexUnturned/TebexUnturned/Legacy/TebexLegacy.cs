using System;
using System.Timers;
using Rocket.API;
using Rocket.Core.Plugins;
using TebexUnturned.Legacy.Models;
using Logger = Rocket.Core.Logging.Logger;

namespace TebexUnturned.Legacy
{
    public class TebexLegacy : RocketPlugin<TebexConfiguration> 
    {
        private static System.Timers.Timer aTimer;
        private DateTime lastCalled = DateTime.Now.AddMinutes(-14);
        public static TebexLegacy Instance;
        public int nextCheck = 15 * 60;
        public WebstoreInfo information;

        private void checkQueue()
        {
            if ((DateTime.Now - this.lastCalled).TotalSeconds > TebexLegacy.Instance.nextCheck)
            {
                this.lastCalled = DateTime.Now;
                /* FIXME
                CommandTebexForcecheck checkCommand = new CommandTebexForcecheck();
                String[] command = new[] { "tebex:forcecheck" };
                checkCommand.Execute(new ConsolePlayer(), command);
                */                
            }
        }

        protected override void Load()
        {
            this.information = new WebstoreInfo();
            Instance = this;
            
            
            logWarning("Tebex Legacy Loaded");
            if (Instance.Configuration.Instance.secret == "")
            {
                logError("You have not yet defined your secret key. Use /tebex:secret <secret> to define your key");
            }
            else
            {
                /* FIXME?
                CommandTebexInfo infoCommand = new CommandTebexInfo();
                String[] command = new[] { "tebex:info" };
                infoCommand.Execute(new ConsolePlayer(), command);
                */
            }

            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, errors) => { return true; };
            
            //UnturnedChatListener unturnedChatListener = new UnturnedChatListener();
            //unturnedChatListener.Register(this);
            SetTimer();
        }

        /*public static void DoCheck()
        {
        }*/

        private void SetTimer()
        {
            int time = Configuration.Instance.CheckIntervalInSeconds * 1000;
            //Don't allow checks shorter than 60 seconds
            if (time < 60000)
                time = 60000;

            aTimer = new System.Timers.Timer(time);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

        }

        public void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (this.isActiveAndEnabled && Instance.Configuration.Instance.secret != "")
                this.checkQueue();
        }

        /*public void FixedUpdate()
        {
            if (this.isActiveAndEnabled)
                this.checkQueue();
        }*/

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