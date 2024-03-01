using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Tebex.Adapters;
using Tebex.API;
using Tebex.Shared.Components;
using Tebex.Triage;
using TebexUnturned;

namespace Tebex.Plugins
{
    public class TebexUnturned : RocketPlugin<BaseTebexAdapter.TebexConfig>
    {
        private static TebexUnturnedAdapter _adapter;
         
        private static IPlayerManager players;
        private static PluginTimers _timers;
        private static WebRequests _webrequest;
        private static IServer server;
        
        public static string GetPluginVersion()
        {
            return "2.0.0-DEV";
        }

        protected override void Load()
        {
            Init();
        }

        private void Init()
        {
            // Setup our API and adapter
            _adapter = new TebexUnturnedAdapter(this);
            _adapter.LogInfo("Tebex is starting up...");
            TebexApi.Instance.InitAdapter(_adapter);

            // Init plugin components so they have access to our adapter
            _webrequest = new WebRequests(_adapter);
            _timers = new PluginTimers(_adapter);
            _timers.Every(0.5f, () =>
            {
                Task task = _webrequest.ProcessNextRequestAsync();
                task.RunSynchronously();
            });
 
            // Check if auto reporting is disabled and show a warning if so.
            if (!BaseTebexAdapter.PluginConfig.AutoReportingEnabled)
            {
                _adapter.LogWarning("Auto reporting issues to Tebex is disabled.");
                _adapter.LogWarning("To enable, please set 'AutoReportingEnabled' to 'true' in config/Tebex.json");
            }

            // Check if secret key has been set. If so, get store information and place in cache
            if (BaseTebexAdapter.PluginConfig.SecretKey != "your-secret-key-here")
            {
                // No-op, just to place info in the cache for any future triage events
                _adapter.FetchStoreInfo((info => { }));
                return;
            }

            _adapter.LogInfo("Tebex detected a new configuration file.");
            _adapter.LogInfo("Use tebex:secret <secret> to add your store's secret key.");
            _adapter.LogInfo("Alternatively, add the secret key to 'Tebex.json' and reload the plugin.");

            UnturnedChatListener chatListener = new UnturnedChatListener();
            chatListener.Register(this, _adapter);
        }

        public WebRequests WebRequests()
        {
            return _webrequest;
        }

        public IPlayerManager PlayerManager()
        {
            return players;
        }

        public PluginTimers PluginTimers()
        {
            return _timers;
        }

        public IServer Server()
        {
            return server;
        }

        public string GetGame()
        {
            return "Unturned";
        }

        public void Warn(string message)
        {
            Logger.LogWarning(message);
        }

        public void Error(string message)
        {
            Logger.LogError(message);
        }

        public void Info(string info)
        {
            Logger.Log(info);
        }

        private void OnUserConnected(IPlayer player)
        {
            // Check for default config and inform the admin that configuration is waiting.
            if (player.IsAdmin && BaseTebexAdapter.PluginConfig.SecretKey == "your-secret-key-here")
            {
                player.AddChat("Tebex is not configured. Use tebex:secret <secret> from the F1 menu to add your key.");
                player.AddChat("Get your secret key by logging in at:");
                player.AddChat("https://tebex.io/");
            }

            _adapter.LogDebug($"Player login event: {player.Id}@{player.Address}");
            _adapter.OnUserConnected(player.Id, player.Address);
        }
        
        private void OnServerShutdown()
        {
            // Make sure join queue is always empties on shutdown
            _adapter.ProcessJoinQueue();
        }

        private void PrintCategories(IPlayer player, List<TebexApi.Category> categories)
        {
            // Index counter for selecting displayed items
            var categoryIndex = 1;
            var packIndex = 1;

            // Line separator for category response
            _adapter.ReplyPlayer(player, "---------------------------------");

            // Sort categories in order and display
            var orderedCategories = categories.OrderBy(category => category.Order).ToList();
            for (int i = 0; i < categories.Count; i++)
            {
                var listing = orderedCategories[i];
                _adapter.ReplyPlayer(player, $"[C{categoryIndex}] {listing.Name}");
                categoryIndex++;

                // Show packages for the category in order from API
                if (listing.Packages.Count > 0)
                {
                    var packages = listing.Packages.OrderBy(category => category.Order).ToList();
                    _adapter.ReplyPlayer(player, $"Packages");
                    foreach (var package in packages)
                    {
                        // Add additional flair on sales
                        if (package.Sale != null && package.Sale.Active)
                        {
                            _adapter.ReplyPlayer(player,
                                $"-> [P{packIndex}] {package.Name} {package.Price - package.Sale.Discount} (SALE {package.Sale.Discount} off)");
                        }
                        else
                        {
                            _adapter.ReplyPlayer(player, $"-> [P{packIndex}] {package.Name} {package.Price}");
                        }

                        packIndex++;
                    }
                }

                // At the end of each category add a line separator
                _adapter.ReplyPlayer(player, "---------------------------------");
            }
        }

        private static void PrintPackages(IPlayer player, List<TebexApi.Package> packages)
        {
            // Index counter for selecting displayed items
            var packIndex = 1;

            _adapter.ReplyPlayer(player, "---------------------------------");
            _adapter.ReplyPlayer(player, "      PACKAGES AVAILABLE         ");
            _adapter.ReplyPlayer(player, "---------------------------------");

            // Sort categories in order and display
            var orderedPackages = packages.OrderBy(package => package.Order).ToList();
            for (var i = 0; i < packages.Count; i++)
            {
                var package = orderedPackages[i];
                // Add additional flair on sales
                _adapter.ReplyPlayer(player, $"[P{packIndex}] {package.Name}");
                _adapter.ReplyPlayer(player, $"Category: {package.Category.Name}");
                _adapter.ReplyPlayer(player, $"Description: {package.Description}");

                if (package.Sale != null && package.Sale.Active)
                {
                    _adapter.ReplyPlayer(player,
                        $"Original Price: {package.Price} {package.GetFriendlyPayFrequency()}  SALE: {package.Sale.Discount} OFF!");
                }
                else
                {
                    _adapter.ReplyPlayer(player, $"Price: {package.Price} {package.GetFriendlyPayFrequency()}");
                }

                _adapter.ReplyPlayer(player,
                    $"Purchase with 'tebex.checkout P{packIndex}' or 'tebex.checkout {package.Id}'");
                _adapter.ReplyPlayer(player, "--------------------------------");

                packIndex++;
            }
        }

        private BaseTebexAdapter.TebexConfig GetDefaultConfig()
        {
            return new BaseTebexAdapter.TebexConfig();
        }

        public static BaseTebexAdapter GetAdapter()
        {
            return _adapter;
        }
    }
}