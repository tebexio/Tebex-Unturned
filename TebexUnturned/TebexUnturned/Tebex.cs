using System;
using System.Timers;
using Rocket.API;
using Rocket.Core.Plugins;
using TebexUnturned.Models;
using Logger = Rocket.Core.Logging.Logger;

namespace TebexUnturned
{
    public class Tebex : RocketPlugin<TebexConfiguration> 
    {
        
        private static System.Timers.Timer aTimer;
        private DateTime lastCalled = DateTime.Now.AddMinutes(-14);
        public static Tebex Instance;
        public int nextCheck = 15 * 60;
        public WebstoreInfo information;

        private void checkQueue()
        {
            if ((DateTime.Now - this.lastCalled).TotalSeconds > Tebex.Instance.nextCheck)
            {
                
                this.lastCalled = DateTime.Now;
                CommandTebexForcecheck checkCommand = new CommandTebexForcecheck();
                String[] command = new[] { "tebex:forcecheck" };
                checkCommand.Execute(new ConsolePlayer(), command);                
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
            
            ChatListener chatListener = new ChatListener();
            chatListener.Register(this);
            SetTimer();
        }

        /*public static void DoCheck()
        {
        }*/

        private void SetTimer()
        {
            int time = Configuration.Instance.CheckIntervalInSeconds * 1000;
            aTimer = new System.Timers.Timer(time);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

        }

        public void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (this.isActiveAndEnabled)
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