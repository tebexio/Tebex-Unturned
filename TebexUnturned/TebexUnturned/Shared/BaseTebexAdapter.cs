using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Rocket.API;
using Tebex.API;
using Tebex.Util;

namespace Tebex.Adapters
{
    /// <summary>
    /// BaseTebexAdapter implements the common logic platform for interacting with the Tebex API. Multiple types of Adapters can
    /// be created which handle logic for specific games, frameworks, or environments.
    /// </summary>
    public abstract class BaseTebexAdapter
    {
        public static BaseTebexAdapter Instance { get; protected set; }
        
        public static TebexConfig PluginConfig { get; set; } = new TebexConfig();
        
        // For our timed functions, this is when each function can run next.
        private static DateTime _nextCheckCommandQueue = DateTime.Now;
        private static DateTime _nextCheckDeleteCommands = DateTime.Now;
        private static DateTime _nextCheckJoinQueue = DateTime.Now;
        private static DateTime _nextCheckRefresh = DateTime.Now;
        
        // Player join and leave events
        private static List<TebexApi.TebexJoinEventInfo> _eventQueue = new List<TebexApi.TebexJoinEventInfo>();
        
        // Successfully executed commands sent to DELETE /commands
        private static readonly List<TebexApi.Command> _executedCommands = new List<TebexApi.Command>();

        // Pauses all web requests when we're rate limited
        public bool IsRateLimited { get; protected set; } = false;
        
        /// <summary>
        /// Init is the main entry point for any Tebex Adapter. All loading, setup, initialization, and timers
        /// be triggered here to enable Tebex.
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Removes any executed commands from the command queue.
        /// </summary>
        /// <param name="ignoreWaitCheck">True if we should ignore any current wait timers. This can lead to rate limiting if enabled.</param>
        public void DeleteExecutedCommands(bool ignoreWaitCheck = false)
        {
            LogDebug("Deleting executed commands...");
            if (!CanProcessNextDeleteCommands() && !ignoreWaitCheck)
            {
                LogDebug("Skipping check for completed commands - not time to be processed");
                return;
            }
            
            if (_executedCommands.Count == 0)
            {
                LogDebug("  No commands to flush.");
                return;
            }

            // Reset next check for deleting commands
            _nextCheckDeleteCommands = DateTime.Now.AddSeconds(60);
            LogDebug($"  Found {_executedCommands.Count} commands to flush.");

            // Build a list of command IDs from the commands we have stored
            List<int> ids = new List<int>();
            foreach (var command in _executedCommands)
            {
                ids.Add(command.Id);
            }
            
            // Send the commands we want to delete to Tebex
            TebexApi.Instance.DeleteCommands(ids.ToArray(), (code, body) =>
            {
                LogDebug("Successfully flushed completed commands.");
                _executedCommands.Clear();
            }, (error) =>
            {
                LogError($"Failed to flush completed commands: {error.ErrorMessage}");
            }, (code, body) =>
            {
                LogError($"Unexpected error while flushing completed commands. API response code {code}. Response body follows:");
                LogError(body);
            });
        }
        
        /// <summary>
        /// Logs a warning to server console and log. An alert is a "warning" if it can be resolved by the user and/or time.
        /// All warnings require a solution which explain to the user what should be done to stop the warning. If enabled,
        /// warnings should be queued and sent to Tebex.
        /// </summary>
        /// <param name="message">User-friendly warning message shown to the user.</param>
        /// <param name="solution">Potential solution which resolves the warning message.</param>
        public abstract void LogWarning(string message, string solution);

        /// <summary>
        /// <see cref="LogWarning(string,string)"/>
        /// </summary>
        /// <param name="message">User-friendly warning message shown to the user.</param>
        /// <param name="solution">Potential solution which resolves the warning message.</param>
        /// <param name="metadata">Data to include with a plugin event sent to Tebex.</param>
        public abstract void LogWarning(string message, string solution, Dictionary<String, String> metadata);

        /// <summary>
        /// Logs an error to server console and log. An alert is an "error" if it cannot be resolved by the user at
        /// runtime. Errors do not require solutions like warnings. If enabled, errors should be queued and sent to Tebex.
        /// </summary>
        /// <param name="message">User-friendly error message shown to the user.</param>
        public abstract void LogError(string message);

        /// <summary>
        /// <see cref="LogError(string)"/>
        /// </summary>
        /// <param name="message">User-friendly error message shown to the user.</param>
        /// <param name="metadata">Data to include with a plugin event sent to Tebex.</param>
        public abstract void LogError(string message, Dictionary<String, String> metadata);
        
        /// <summary>
        /// Logs general information to the console and game log.
        /// </summary>
        /// <param name="message">User-friendly message to show to the user.</param>
        public abstract void LogInfo(string message);

        /// <summary>
        /// If debug mode is enabled, logs information to the console and game log.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public abstract void LogDebug(string message);

        /// <summary>
        /// Records a user joining a server.
        /// </summary>
        /// <param name="accountId">steam64Id or other identifying information about the user who joined</param>
        /// <param name="ip">The joining user's IP address</param>
        public void OnUserConnected(string accountId, string ip)
        {
            var joinEvent = new TebexApi.TebexJoinEventInfo(accountId, "server.join", DateTime.Now, ip);
            _eventQueue.Add(joinEvent);
            
            // Joins are cleared on a timer but are also sent at max 10 at a time to prevent huge requests for large servers.
            if (_eventQueue.Count > 10) 
            {
                ProcessJoinQueue(true);
            }
        }
        
        /// <summary>
        /// Main configuration class for a Tebex integration using the Tebex Adapter.
        /// </summary>
        public class TebexConfig : IRocketPluginConfiguration
        {
            /// <summary>
            /// Enables additional debug logging. Raw user info may be written to file and or console.
            /// </summary>
            public bool DebugMode = false;

            /// <summary>
            /// If true, any Warning events written won't be sent to Tebex
            /// </summary>
            public bool SuppressWarnings = false;

            /// <summary>
            /// If true, any Error events written won't be sent to Tebex.
            /// </summary>
            public bool SuppressErrors = false;
            
            public bool AutoReportingEnabled = true;
            
            /// <summary>
            /// The store's Tebex secret key
            /// </summary>
            public string SecretKey = "";
            
            /// <summary>
            /// In minutes, how long any cached objects are valid for by default
            /// </summary>
            public int CacheLifetime = 30;

            public string CustomBuyCommand = "";
            public bool BuyEnabled = false;
            
            public void LoadDefaults()
            {
                DebugMode = false;
                AutoReportingEnabled = true;
                SecretKey = "your-secret-key-here";
                CacheLifetime = 30;
            }
        }
        
        /// <summary>
        /// Standard key-value Cache implemented by an underlying Dictionary. Contains <see cref="CachedObject"/>s which wrap the
        /// original object around an expiration time.
        /// </summary>
        public class Cache
        {
            public static Cache Instance => _cacheInstance.Value;
            private static readonly Lazy<Cache> _cacheInstance = new Lazy<Cache>(() => new Cache());
            private static Dictionary<string, CachedObject> _cache = new Dictionary<string, CachedObject>();
            public CachedObject Get(string key)
            {
                if (_cache.ContainsKey(key))
                {
                    return _cache[key];
                }
                return null;
            }

            public void Set(string key, CachedObject obj)
            {
                _cache[key] = obj;
            }

            public bool HasValid(string key)
            {
                return _cache.ContainsKey(key) && !_cache[key].HasExpired();
            }

            public void Clear()
            {
                _cache.Clear();
            }

            public void Remove(string key)
            {
                _cache.Remove(key);
            }
        }

        /// <summary>
        /// An object stored in the cache. Contains an expiration time after which the object is no longer considered valid.
        /// </summary>
        public class CachedObject
        {
            /// <summary>
            /// The object that was originally cached
            /// </summary>
            public object Value { get; }
            private readonly DateTime _expires;

            public CachedObject(object obj, int minutesValid)
            {
                Value = obj;
                _expires = DateTime.Now.AddMinutes(minutesValid);
            }

            public bool HasExpired()
            {
                return DateTime.Now > _expires;
            }
        }
        
        #region Callback Types
        
        // Callback types representing responses received from various API calls
        public delegate void CreateCheckoutUrlResponse(TebexApi.CheckoutUrlPayload checkoutUrl);
        public delegate void GetGiftCardsResponse(List<TebexApi.GiftCard> giftCards);
        public delegate void GetGiftCardByIdResponse(TebexApi.GiftCard giftCards);
        public delegate void FetchStoreInfoResponse(TebexApi.TebexStoreInfo info);
        public delegate void GetCategoriesResponse(List<TebexApi.Category> categories);
        public delegate void GetPackagesResponse(List<TebexApi.Package> packages);
        
        #endregion

        /// <summary>
        /// Retrieves the <see cref="TebexApi.TebexStoreInfo"/> associated with our store. If store information is already
        /// cached and still valid, the cached information is returned.
        /// </summary>
        /// <param name="response">Handler function for successful information retrieval</param>
        /// <param name="apiErrorCallback">Handler function for when an API error occurs (wrong key)</param>
        public void FetchStoreInfo(FetchStoreInfoResponse response, TebexApi.ApiErrorCallback apiErrorCallback)
        {
            if (Cache.Instance.HasValid("information"))
            {
                response?.Invoke((TebexApi.TebexStoreInfo)Cache.Instance.Get("information").Value);
            }
            else
            {
                // Query Tebex API for store information
                TebexApi.Instance.Information((code, body) =>
                {
                    // Convert received response to TebexStoreInfo 
                    var storeInfo = JsonConvert.DeserializeObject<TebexApi.TebexStoreInfo>(body);
                    if (storeInfo == null)
                    {
                        LogError("Failed to parse fetched store information!", new Dictionary<string, string>()
                        {
                            {"response", body},
                        });
                        return;
                    }
                    
                    // Our response handler function will be passed the store info for handling
                    Cache.Instance.Set("information", new CachedObject(storeInfo, PluginConfig.CacheLifetime));
                    response?.Invoke(storeInfo);
                }, apiErrorCallback);
            }
        }

        /// <summary>
        /// Retrieves the <see cref="TebexApi.Package"> associated with the given short code or ID value./>
        /// </summary>
        /// <param name="value">A short code (P1, P2, etc.) or package ID (0123127244)</param>
        /// <returns>The package associated with the given code, or null if the package was not found.</returns>
        public TebexApi.Package GetPackageByShortCodeOrId(string value)
        {
            var shortCodes = (Dictionary<String, TebexApi.Package>)Cache.Instance.Get("packageShortCodes").Value;
            if (shortCodes.ContainsKey(value))
            {
                return shortCodes[value];
            }

            // No short code found, assume it's a package ID
            var packages = (List<TebexApi.Package>)Cache.Instance.Get("packages").Value;
            foreach (var package in packages)
            {
                if (package.Id.ToString() == value)
                {
                    return package;
                }
            }
            
            return null; //FIXME cleaner null handling
        }
        
        /// <summary>
        /// Refreshes cached categories and packages from the Tebex API. Can be used by commands or with no arguments to
        /// update the information while the server is idle.
        /// </summary>
        /// <param name="onSuccess">
        /// Handler function triggered when a successful response is received. This is a basic API success including
        /// the HTTP response code and response body which can be deserialized into the appropriate data type.
        /// </param>
        public void RefreshListings(TebexApi.ApiSuccessCallback onSuccess = null)
        {
            // Get our categories from the /listing endpoint as it contains all category data
            TebexApi.Instance.GetListing((code, body) =>
            {
                var response = JsonConvert.DeserializeObject<TebexApi.ListingsResponse>(body);
                if (response == null)
                {
                    LogError("Could not get refresh all listings!", new Dictionary<string, string>()
                    {
                        {"response", body},
                    });
                    return;
                }

                Cache.Instance.Set("categories", new CachedObject(response.categories, PluginConfig.CacheLifetime));
                if (onSuccess != null)
                {
                    onSuccess.Invoke(code, body);    
                }
            });

            // Get our packages from a verbose get all packages call so that we always have the description
            // of the package cached.
            TebexApi.Instance.GetAllPackages(true, (code, body) =>
            {
                var response = JsonConvert.DeserializeObject<List<TebexApi.Package>>(body);
                if (response == null)
                {
                    LogError("Could not refresh package listings!", new Dictionary<string, string>()
                    {
                        {"response", body}
                    });
                    return;
                }

                Cache.Instance.Set("packages", new CachedObject(response, PluginConfig.CacheLifetime));

                /*
                 * Generates and saves shortcodes for each package. Shortcodes are "P1", "P2", etc...
                 * Packages can be displayed and purchased with their IDs or shortcodes.
                 * Shortcodes are generated by the order of the packages which can be set by the user in their Tebex store panel.
                 */
                var orderedPackages = response.OrderBy(package => package.Order).ToList();
                var shortCodes = new Dictionary<String, TebexApi.Package>();
                for (var i = 0; i < orderedPackages.Count; i++)
                {
                    var package = orderedPackages[i];
                    shortCodes.Add($"P{i + 1}", package);
                }

                Cache.Instance.Set("packageShortCodes", new CachedObject(shortCodes, PluginConfig.CacheLifetime));
                onSuccess?.Invoke(code, body);
            });
        }
        
        /// <summary>
        /// Gets all package Categories associated with the store. The response is saved in the cache. Cached data is returned if it's still valid.
        /// </summary>
        /// <param name="onSuccess">Handler function which receives a List of <see cref="TebexApi.Category"/> on success.</param>
        /// <param name="onServerError">Optional handler function triggered when a server error is received.</param>
        public void GetCategories(GetCategoriesResponse onSuccess,
            TebexApi.ServerErrorCallback onServerError = null)
        {
            if (Cache.Instance.HasValid("categories"))
            {
                onSuccess.Invoke((List<TebexApi.Category>)Cache.Instance.Get("categories").Value);
            }
            else
            {
                TebexApi.Instance.GetListing((code, body) =>
                {
                    var response = JsonConvert.DeserializeObject<TebexApi.ListingsResponse>(body);
                    if (response == null)
                    {
                        onServerError?.Invoke(code, body);
                        return;
                    }

                    Cache.Instance.Set("categories", new CachedObject(response.categories, PluginConfig.CacheLifetime));
                    onSuccess.Invoke(response.categories);
                });
            }
        }

        /// <summary>
        /// Gets all <see cref="TebexApi.Package"/>s associated with the store.
        /// </summary>
        /// <param name="onSuccess">Handler function wich receives a List of <see cref="TebexApi.Package"/> on success.</param>
        /// <param name="onServerError">Optional handler function triggered when server error is received.</param>
        public void GetPackages(GetPackagesResponse onSuccess,
            TebexApi.ServerErrorCallback onServerError = null)
        {
            try
            {
                if (Cache.Instance.HasValid("packages"))
                {
                    onSuccess.Invoke((List<TebexApi.Package>)Cache.Instance.Get("packages").Value);
                }
                else
                {
                    // RefreshListings will update both packages and shortcodes in the cache
                    RefreshListings((code, body) =>
                    {
                        onSuccess.Invoke((List<TebexApi.Package>)Cache.Instance.Get("packages").Value);
                    });
                }
            }
            catch (Exception e)
            {
                LogError("An error occurred while getting your store's packages. " + e.Message, new Dictionary<string, string>()
                {
                    {"trace", e.StackTrace},
                    {"message", e.Message}
                });
            }
        }

        /// <summary>
        /// RefreshStoreInformation synchronizes all store information, categories, and packages between the Tebex Store and this integration.
        /// </summary>
        /// <param name="ignoreWaitCheck">Whether we ignore the refresh time limit. Can lead to rate limiting if enabled.</param>
        public void RefreshStoreInformation(bool ignoreWaitCheck = false)
        {
            LogDebug("Refreshing store information...");
            
            // Calling places the information in the cache
            if (!CanProcessNextRefresh() && !ignoreWaitCheck)
            {
                LogDebug("  Skipping store info refresh - not time to be processed");
                return;
            }
            
            _nextCheckRefresh = DateTime.Now.AddMinutes(15);
            FetchStoreInfo(info => { }, (error) =>
            {
                LogError("Error while refreshing store information: " + error.ErrorMessage);
            });
        }
        
        /// <summary>
        /// ProcessJoinQueue will send any <see cref="TebexApi.PlayerJoinEvent"/>s in the event queue to Tebex.
        /// </summary>
        /// <param name="ignoreWaitCheck">Whether we can ignore the join queue time limit. Can lead to rate limiting if enabled.</param>
        public void ProcessJoinQueue(bool ignoreWaitCheck = false)
        {
            LogDebug("Processing player join queue...");
            
            if (!CanProcessNextJoinQueue() && !ignoreWaitCheck)
            {
                LogDebug("  Skipping join queue - not time to be processed");
                return;
            }
            
            _nextCheckJoinQueue = DateTime.Now.AddSeconds(60);
            if (_eventQueue.Count > 0)
            {
                LogDebug($"  Found {_eventQueue.Count} join events.");
                TebexApi.Instance.PlayerJoinEvent(_eventQueue, (code, body) =>
                    {
                        LogDebug("Join queue cleared successfully.");
                        _eventQueue.Clear();
                    }, error =>
                    {
                        LogError($"Could not process join queue - error response from API: {error.ErrorMessage}");
                    },
                    (code, body) =>
                    {
                        LogError("Could not process join queue - unexpected server error.", new Dictionary<string, string>()
                        {
                            {"response", body},
                            {"code", code.ToString()},
                        });
                    });
            }
            else // Empty queue
            {
                LogDebug($"  No recent join events.");
            }
        }
        
        /// <summary>
        /// ProcessCommandQueue retrieves pending offline (instant) and online (player must be logged in) commands from Tebex and executes them. 
        /// </summary>
        /// <param name="ignoreWaitCheck">Whether we can ignore the command queue time limit. Can lead to rate limiting if enabled.</param>
        public void ProcessCommandQueue(bool ignoreWaitCheck = false)
        {
            LogDebug("Processing command queue...");
            
            if (!CanProcessNextCommandQueue() && !ignoreWaitCheck)
            {
                var secondsToWait = (int)(_nextCheckCommandQueue - DateTime.Now).TotalSeconds;
                LogDebug($"  Tried to run command queue, but should wait another {secondsToWait} seconds.");
                return;
            }

            // Get the current state of the command queue from Tebex.
            TebexApi.Instance.GetCommandQueue((cmdQueueCode, cmdQueueResponseBody) =>
            {
                var commandQueue = JsonConvert.DeserializeObject<TebexApi.CommandQueueResponse>(cmdQueueResponseBody);
                if (commandQueue == null)
                {
                    LogError("Failed to get command queue. Could not parse response from API.", new Dictionary<string, string>()
                    {
                        {"response", cmdQueueResponseBody},
                        {"code", cmdQueueCode.ToString()},
                    });
                    return;
                }

                /*
                 * On a successful response the API will tell us when we can perform our next check (after x seconds).
                 * This can change and should be respected in order to avoid rate limits.
                 */
                _nextCheckCommandQueue = DateTime.Now.AddSeconds(commandQueue.Meta.NextCheck);

                /*
                 * Process any offline commands first. Offline commands can be run instantly without requiring that a player
                 * be on the server for a command to be executed.
                 */
                if (!commandQueue.Meta.ExecuteOffline)
                {
                    LogDebug("No offline commands to execute.");
                }
                else // We have offline commands to execute.
                {
                    LogDebug("Requesting offline commands from API...");
                    
                    // Offline commands are a one-shot request that returns all pending offline commands.
                    TebexApi.Instance.GetOfflineCommands((code, offlineCommandsBody) =>
                    {
                        var offlineCommands = JsonConvert.DeserializeObject<TebexApi.OfflineCommandsResponse>(offlineCommandsBody);
                        if (offlineCommands == null)
                        {
                            LogError("Failed to get offline commands. Could not parse response from API.", new Dictionary<string, string>()
                            {
                                {"code", code.ToString()},
                                {"responseBody", offlineCommandsBody}
                            });
                            return;
                        }

                        // Deserialized OfflineCommandsResponse will contain the offline commands we should run and on who.
                        LogDebug($"Found {offlineCommands.Commands.Count} offline commands to execute.");
                        foreach (TebexApi.Command command in offlineCommands.Commands)
                        {
                            // Each integration can implement its own handling of offline variables if variable tags are not filled by the API
                            var parsedCommand = ExpandOfflineVariables(command.CommandToRun, command.Player);

                            // We split the parsed command into its components to be passed to the integration
                            var splitCommand = parsedCommand.Split(' ');
                            var commandName = splitCommand[0];
                            var args = splitCommand.Skip(1);
                            
                            LogDebug($"Executing offline command: `{parsedCommand}`");
                            
                            // ExecuteOfflineCommand will be implemented by the integration
                            ExecuteOfflineCommand(command, commandName, args.ToArray());
                            
                            _executedCommands.Add(command); //FIXME all offline commands are automatically marked successful
                        }
                        LogDebug($"Executed commands queue has {_executedCommands.Count} commands");
                    }, (error) => // API error from offline commands
                    {
                        LogError($"Error response from API while processing offline commands: {error.ErrorMessage}", new Dictionary<string, string>()
                        {
                            {"error",error.ErrorMessage},
                            {"errorCode", error.ErrorCode.ToString()}
                        });
                    }, (offlineComandsCode, offlineCommandsServerError) => // Server error from offline commands
                    {
                        LogError("Unexpected error response from API while processing offline commands", new Dictionary<string, string>()
                        {
                            {"code", offlineComandsCode.ToString()},
                            {"responseBody", offlineCommandsServerError}
                        });
                    });
                }

                /*
                 * Online commands are processed per player. Each player requires a request to the API to check for their
                 * due commands.
                 */
                LogDebug($"Found {commandQueue.Players.Count} due players in the queue");
                foreach (var duePlayer in commandQueue.Players)
                {
                    LogDebug($"Processing online commands for player {duePlayer.Name}...");
                    
                    // IsPlayerOnline is implemented by the integration and will implement logic for checking player online status
                    if (!IsPlayerOnline(duePlayer))
                    {
                        LogDebug($"> Player {duePlayer.Name} has online commands but is not connected. Skipping.");
                        continue;
                    }
                    
                    // When the player is online, we ask Tebex for the online commands pending for that player.
                    TebexApi.Instance.GetOnlineCommands(duePlayer.Id,
                        (onlineCommandsCode, onlineCommandsResponseBody) =>
                        {
                            LogDebug(onlineCommandsResponseBody);
                            var onlineCommands =
                                JsonConvert.DeserializeObject<TebexApi.OnlineCommandsResponse>(
                                    onlineCommandsResponseBody);
                            if (onlineCommands == null)
                            { 
                                LogError($"> Failed to get online commands for ${duePlayer.Name}. Could not unmarshal response from API.", new Dictionary<string, string>()
                                {
                                    {"playerName", duePlayer.Name},
                                    {"code", onlineCommandsCode.ToString()},
                                    {"responseBody", onlineCommandsResponseBody}
                                });
                                return;
                            }

                            LogDebug($"> Processing {onlineCommands.Commands.Count} commands for this player...");
                            foreach (var command in onlineCommands.Commands)
                            {
                                /*
                                 * The playerRef represents a reference to the player in the game. In some games such as
                                 * Conan Exiles, we refer to the player not by their Id or Username, but their position in the
                                 * online players list retrieved via RCON.
                                 *
                                 * This is used to modify how commands are directed to players by each integration.
                                 */
                                object playerRef = GetPlayerRef(onlineCommands.Player.Id);
                                if (playerRef == null)
                                {
                                    LogError($"No reference found for expected online player. Commands will be skipped for this player.");
                                    break;
                                }

                                // Each integration will implement their own ExpandUserameVariables
                                var parsedCommand = ExpandUsernameVariables(command.CommandToRun, duePlayer);
                                var splitCommand = parsedCommand.Split(' ');
                                var commandName = splitCommand[0];
                                var args = splitCommand.Skip(1);
                                
                                LogDebug($"Pre-execution: {parsedCommand}");
                                
                                //ExecuteOnlineCommand is implemented by each integration
                                var success = ExecuteOnlineCommand(command, duePlayer, commandName, args.ToArray());
                                
                                LogDebug($"Post-execution: {parsedCommand}");
                                if (success)
                                {
                                    _executedCommands.Add(command);    
                                }
                            }
                        }, tebexError => // Error for this player's online commands
                        {
                            LogError("Failed to get due online commands due to error response from API.", new Dictionary<string, string>()
                            {
                                {"playerName", duePlayer.Name},
                                {"code", tebexError.ErrorCode.ToString()},
                                {"message", tebexError.ErrorMessage}
                            });
                        });
                }
            }, tebexError => // Error for get due players
            {
                LogError("Failed to get due players due to error response from API.", new Dictionary<string, string>()
                {
                    {"code", tebexError.ErrorCode.ToString()},
                    {"message", tebexError.ErrorMessage}
                });
            });
        }
        
        /// <summary>
        /// Creates a payment URL for a provided package on behalf of a specific player.
        /// </summary>
        /// <param name="playerName">The player's username purchasing the package.</param>
        /// <param name="package">The package to purchase.</param>
        /// <param name="success">Handler for successful requests, provides the checkout URL and an expiry. <see cref="TebexApi.CheckoutUrlPayload"/></param>
        /// <param name="error">Handler for API errors (package doesn't exist)</param>
        public void CreateCheckoutUrl(string playerName, TebexApi.Package package,
            CreateCheckoutUrlResponse success,
            TebexApi.ApiErrorCallback error)
        {
            TebexApi.Instance.CreateCheckoutUrl(package.Id, playerName, (code, body) =>
            {
                var responsePayload = JsonConvert.DeserializeObject<TebexApi.CheckoutUrlPayload>(body);
                if (responsePayload == null)
                {
                    return;
                }

                success?.Invoke(responsePayload);
            }, error);
        }
        
        public void GetGiftCards(GetGiftCardsResponse success, TebexApi.ApiErrorCallback error)
        {
            //TODO
        }

        public void GetGiftCardById(GetGiftCardByIdResponse success, TebexApi.ApiErrorCallback error)
        {
            //TODO
        }

        /// <summary>
        /// Creates a ban for a player using the Tebex API. Players cannot be unbanned via the API and must be unbanned
        /// from the Tebex webstore.
        /// </summary>
        /// <param name="playerName">The player's username.</param>
        /// <param name="playerIp">The player's IP address.</param>
        /// <param name="reason">A reason for the ban.</param>
        /// <param name="onSuccess">
        /// Handler function triggered when a successful response is received. This is a basic API success including
        /// the HTTP response code and response body which can be deserialized into the appropriate data type.
        /// </param>
        /// <param name="onError">Handler function triggered when an API error occurs. A <see cref="TebexApi.TebexError"/> is provided.</param>
        public void BanPlayer(string playerName, string playerIp, string reason, TebexApi.ApiSuccessCallback onSuccess,
            TebexApi.ApiErrorCallback onError)
        {
            TebexApi.Instance.CreateBan(reason, playerIp, playerName, onSuccess, onError);
        }

        /// <summary>
        /// Gets a user's information by ID using the Tebex API.
        /// </summary>
        /// <param name="userId">The player's ID or username.</param>
        /// <param name="onSuccess">
        /// Handler function triggered when a successful response is received. This is a basic API success including
        /// the HTTP response code and response body which can be deserialized into the appropriate data type.
        /// </param>
        /// <param name="onApiError">Handler function triggered when an API error occurs. A <see cref="TebexApi.TebexError"/> is provided.</param>
        /// <param name="onServerError">Handler function triggered when a server error occurs. The response code and body are provided.</param>
        public void GetUser(string userId, TebexApi.ApiSuccessCallback onSuccess = null,
            TebexApi.ApiErrorCallback onApiError = null, TebexApi.ServerErrorCallback onServerError = null)
        {
            TebexApi.Instance.GetUser(userId, onSuccess, onApiError, onServerError);
        }

        /// <summary>
        /// Gets a list of the active packages associated with a customer. A list of <see cref="TebexApi.Package"/> is returned./>
        /// </summary>
        /// <param name="playerId">The player's ID or username.</param>
        /// <param name="packageId">To check if a specific package is active, include its ID</param>
        /// <param name="onSuccess">
        /// Handler function triggered when a successful response is received. This is a basic API success including
        /// the HTTP response code and response body which can be deserialized into the appropriate data type.
        /// </param>
        /// <param name="onApiError">Handler function triggered when an API error occurs. A <see cref="TebexApi.TebexError"/> is provided.</param>
        /// <param name="onServerError">Handler function triggered when a server error occurs. The response code and body are provided.</param>
        public void GetActivePackagesForCustomer(string playerId, int? packageId = null, TebexApi.ApiSuccessCallback onSuccess = null,
            TebexApi.ApiErrorCallback onApiError = null, TebexApi.ServerErrorCallback onServerError = null)
        {
            TebexApi.Instance.GetActivePackagesForCustomer(playerId, packageId, onSuccess, onApiError, onServerError);
        }

        /// <summary>
        /// ExecuteOfflineCommand performs the actions necessary for running a command on the server without checking player online status.
        /// </summary>
        /// <param name="command">A reference to the Tebex API command to run.</param>
        /// <param name="commandName">The name of the command being run.</param>
        /// <param name="args">A list of the command's arguments.</param>
        public abstract bool ExecuteOfflineCommand(TebexApi.Command command, string commandName, string[] args);
        
        /// <summary>
        /// ExecuteOnlineCommand performs a command against an online player. The player's online status should already be
        /// validated at this point.
        /// </summary>
        /// <param name="command">A reference to the Tebex API command to run.</param>
        /// <param name="player">A reference to the <see cref="TebexApi.DuePlayer"/> associated with this command.</param>
        /// <param name="commandName">The name of the command we're running.</param>
        /// <param name="args">A list of the command's arguments.</param>
        /// <returns>True if the command succeeded.</returns>
        public abstract bool ExecuteOnlineCommand(TebexApi.Command command, TebexApi.DuePlayer player, string commandName, string[] args);
        
        /// <summary>
        /// IsPlayerOnline is implemented by the integration and will run the commands necessary to determine if a given
        /// DuePlayer is currently on the server.
        /// </summary>
        /// <param name="duePlayer">A reference to a player that has commands due.</param>
        /// <returns>True if the player is currently online.</returns>
        public abstract bool IsPlayerOnline(TebexApi.DuePlayer duePlayer);

        /// <summary>
        /// GetPlayerRef is implemented by the integration, and should return the appropriate object associated with the player in-game.
        /// In environments running external to a game (RCON) this can be overridden to enable additional handling for how commands are directed
        /// to specific players.
        ///
        /// In some games such as Conan Exiles, we refer to the player not by their Id or Username, but their position
        /// in the online players list retrieved via RCON. In that case GetPlayerRef returns the "Idx" or the player position in this list
        /// which is used as the player's {id} for necessary commands.
        /// </summary>
        /// <param name="playerId">The player's current identifier, either a UUID or username.</param>
        /// <returns>Reference to the in-game player object, if applicable, or another object to use to identify this player.</returns>
        public abstract object GetPlayerRef(string playerId);

        /// <summary>
        /// SaveConfig persists the given configuration instance.
        /// </summary>
        /// <param name="config"></param>
        public abstract void SaveConfig(TebexConfig config);
        
        /// <summary>
        /// ExpandUsernameVariables replaces any variables in our input with the information associated with our DuePlayer.
        /// As we support the use of different games across the Tebex Store we offer slightly different ways of getting a customer username or their ID.
        /// All games support the same default variables, but some games may have additional variables implemented by their integration.
        /// </summary>
        /// <param name="input">The actual command string to be run. Will contain tags to be replaced such as {id}</param>
        /// <param name="player">A reference to the DuePlayer this command is being run on.</param>
        /// <returns>Parsed command string with username variables replaced.</returns>
        public abstract string ExpandUsernameVariables(string input, TebexApi.DuePlayer player);

        /// <summary>
        /// ExpandOfflineVariables replaces any variables in our input command with the information associated with the
        /// offline PlayerInfo object.
        /// </summary>
        /// <param name="input">The actual command string being run. Will contain tags to be replaced such as {id}. Ex: "say {id} hello"</param>
        /// <param name="info">A reference to the PlayerInfo this command is being run on.</param>
        /// <returns>Parsed command string with username variables replaced. Ex: "say TebexDev hello"</returns>
        public abstract string ExpandOfflineVariables(string input, TebexApi.PlayerInfo info);
        
        /// <summary>
        /// Makes an HTTP request to Tebex. Implemented by each integration as some frameworks require usage of their
        /// web functions instead of allowing the standard library.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="body"></param>
        /// <param name="verb"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onApiError"></param>
        /// <param name="onServerError"></param>
        public abstract void MakeWebRequest(string endpoint, string body, TebexApi.HttpVerb verb,
            TebexApi.ApiSuccessCallback onSuccess, TebexApi.ApiErrorCallback onApiError,
            TebexApi.ServerErrorCallback onServerError);

        /// <summary>
        /// IsTebexReady should return true when the environment is in a sufficient state to retrieve and execute commands
        /// from Tebex.
        /// </summary>
        /// <returns>True if able to process Tebex commands.</returns>
        public bool IsTebexReady()
        {
            return PluginConfig.SecretKey != null && !PluginConfig.SecretKey.Equals("") &&
                   !PluginConfig.SecretKey.Equals("your-secret-key-here");
        }
        
        public bool CanProcessNextCommandQueue()
        {
            return DateTime.Now > _nextCheckCommandQueue;
        }

        public bool CanProcessNextDeleteCommands()
        {
            return DateTime.Now > _nextCheckDeleteCommands;
        }
        
        public bool CanProcessNextJoinQueue()
        {
            return DateTime.Now > _nextCheckJoinQueue;
        }
        
        public bool CanProcessNextRefresh()
        {
            return DateTime.Now > _nextCheckRefresh;
        }
        
        public string Success(string message)
        {
            return $"[ {Ansi.Green("\u2713")} ] " + message;
        }

        public string Error(string message)
        {
            return $"[ {Ansi.Red("X")} ] " + message;
        }

        public string Warn(string message)
        {
            return $"[ {Ansi.Yellow("\u26a0")} ] " + message;
        }
    }
}