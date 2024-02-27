using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Tebex.Adapters;

namespace Tebex.API
{
    public class TebexApi
    {
        public static readonly string TebexApiBase = "https://plugin.tebex.io/";
        public static readonly string TebexTriageUrl = "https://plugin-logs.tebex.io/";
        
        public static TebexApi Instance => _apiInstance.Value;
        public static BaseTebexAdapter Adapter { get; private set; }

        // Singleton instance for the API
        private static readonly Lazy<TebexApi> _apiInstance = new Lazy<TebexApi>(() => new TebexApi());

        public TebexApi()
        {
        }

        public void InitAdapter(BaseTebexAdapter adapter)
        {
            Adapter = adapter;
            adapter.Init();
        }
        
        // Used so that we don't depend on Oxide
        public enum HttpVerb
        {
            DELETE,
            GET,
            PATCH,
            POST,
            PUT,
        }

        public delegate void ApiSuccessCallback(int code, string body);

        public delegate void ApiErrorCallback(TebexError error);

        public delegate void ServerErrorCallback(int code, string body);

        public class TebexError
        {
            [JsonProperty("error_code")] public int ErrorCode { get; set; }
            [JsonProperty("error_message")] public string ErrorMessage { get; set; } = "";
        }

        private static void Send(string endpoint, string body, HttpVerb method = HttpVerb.GET,
            ApiSuccessCallback onSuccess = null, ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            Adapter.MakeWebRequest(TebexApiBase + endpoint, body, method, onSuccess, onApiError, onServerError);
        }

        #region Events
        
        public class TebexJoinEventInfo
        {
            [JsonProperty("username_id")] /* steam64ID */
            public string UsernameId { get; private set; }
            [JsonProperty("event_type")]
            public string EventType { get; private set; }
            [JsonProperty("event_date")]
            public DateTime EventDate { get; private set; }
            [JsonProperty("ip")]
            public string IpAddress { get; private set; }

            public TebexJoinEventInfo(string usernameId, string eventType, DateTime eventDate, string ipAddress)
            {
                UsernameId = usernameId;
                EventType = eventType;
                EventDate = eventDate;
                IpAddress = ipAddress;
            }
        }
        
        public void PlayerJoinEvent(List<TebexJoinEventInfo> events, ApiSuccessCallback onSuccess, ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            Send("events", JsonConvert.SerializeObject(events), HttpVerb.POST, onSuccess, onApiError,
                onServerError);
        }
        
        #endregion
        
        #region Information

        public class TebexAccountInfo
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("domain")] public string Domain { get; set; } = "";
            [JsonProperty("name")] public string Name { get; set; } = "";
            [JsonProperty("currency")] public TebexCurrency Currency { get; set; }
            [JsonProperty("online_mode")] public bool OnlineMode { get; set; }
            [JsonProperty("game_type")] public string GameType { get; set; } = "";
            [JsonProperty("log_events")] public bool LogEvents { get; set; }
        }

        public class TebexCurrency
        {
            [JsonProperty("iso_4217")] public string Iso4217 { get; set; } = "";
            [JsonProperty("symbol")] public string Symbol { get; set; } = "";
        }

        public class TebexServerInfo
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("name")] public string Name { get; set; } = "";
        }

        public class TebexStoreInfo
        {
            [JsonProperty("account")] public TebexAccountInfo AccountInfo { get; set; }
            [JsonProperty("server")] public TebexServerInfo ServerInfo { get; set; }
        }

        public class Category
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("order")] public int Order { get; set; }
            [JsonProperty("name")] public string Name { get; set; } = "";
            [JsonProperty("only_subcategories")] public bool OnlySubcategories { get; set; }
            [JsonProperty("subcategories")] public List<Category> Subcategories { get; set; }
            [JsonProperty("packages")] public List<Package> Packages { get; set; }
            [JsonProperty("gui_item")] public object GuiItem { get; set; }
        }

        public class PackageSaleData
        {
            [JsonProperty("active")] public bool Active { get; set; }
            [JsonProperty("discount")] public double Discount { get; set; }
        }

        public class Package
        {
            [JsonProperty("id")] public int Id { get; set; }

            [JsonProperty("name")] public string Name { get; set; } = "";

            [JsonProperty("order")] public string Order { get; set; } = "";

            [JsonProperty("image")] public string Image { get; set; }

            [JsonProperty("price")] public double Price { get; set; }

            [JsonProperty("sale")] public PackageSaleData Sale { get; set; }

            [JsonProperty("expiry_length")] public int ExpiryLength { get; set; }

            [JsonProperty("expiry_period")] public string ExpiryPeriod { get; set; } = "";

            [JsonProperty("type")] public string Type { get; set; } = "";

            [JsonProperty("category")] public Category Category { get; set; }

            [JsonProperty("global_limit")] public int GlobalLimit { get; set; }

            [JsonProperty("global_limit_period")] public string GlobalLimitPeriod { get; set; } = "";

            [JsonProperty("user_limit")] public int UserLimit { get; set; }

            [JsonProperty("user_limit_period")] public string UserLimitPeriod { get; set; } = "";

            [JsonProperty("servers")] public List<TebexServerInfo> Servers { get; set; }

            [JsonProperty("required_packages")] public List<object> RequiredPackages { get; set; } //TODO

            [JsonProperty("require_any")] public bool RequireAny { get; set; }

            [JsonProperty("create_giftcard")] public bool CreateGiftcard { get; set; }

            [JsonProperty("show_until")] public string ShowUntil { get; set; }

            [JsonProperty("gui_item")] public string GuiItem { get; set; } = "";

            [JsonProperty("disabled")] public bool Disabled { get; set; }

            [JsonProperty("disable_quantity")] public bool DisableQuantity { get; set; }

            [JsonProperty("custom_price")] public bool CustomPrice { get; set; }

            [JsonProperty("choose_server")] public bool ChooseServer { get; set; }

            [JsonProperty("limit_expires")] public bool LimitExpires { get; set; }

            [JsonProperty("inherit_commands")] public bool InheritCommands { get; set; }

            [JsonProperty("variable_giftcard")] public bool VariableGiftcard { get; set; }

            // Description is not provided unless verbose=true is passed to the Packages endpoint
            [JsonProperty("description")] public string Description { get; set; } = "";

            public string GetFriendlyPayFrequency()
            {
                switch (Type)
                {
                    case "single": return "One-Time";
                    case "subscription": return $"Each {ExpiryLength} {ExpiryPeriod}";
                    default: return "???";
                }
            }
        }

        // Data returned when sending a package to /checkout
        public class CheckoutUrlPayload
        {
            [JsonProperty("url")] public string Url { get; set; } = "";
            [JsonProperty("expires")] public string Expires { get; set; } = "";
        }

        public delegate void Callback(int code, string body);

        public void Information(ApiSuccessCallback success, ApiErrorCallback error = null)
        {
            Send("information", "", HttpVerb.GET, success, error);
        }

        #endregion

        #region Command Queue

        /**
             * Response received from /queue
             */
        public class CommandQueueResponse
        {
            [JsonProperty("meta")] public CommandQueueMeta Meta { get; set; }
            [JsonProperty("players")] public List<DuePlayer> Players { get; set; }
        }

        /**
             * Metadata received from /queue
             */
        public class CommandQueueMeta
        {
            [JsonProperty("execute_offline")] public bool ExecuteOffline { get; set; }

            [JsonProperty("next_check")] public int NextCheck { get; set; }

            [JsonProperty("more")] public bool More { get; set; }
        }

        /**
             * A due player is one returned by /queue to indicate we have some commands to run.
             */
        public class DuePlayer
        {
            [JsonProperty("id")] public int Id { get; set; }

            [JsonProperty("name")] public string Name { get; set; } = "";

            [JsonProperty("uuid")] public string UUID { get; set; } = "";
        }


        /**
             * The response recieved from /queue/online-commands
             */
        public class OnlineCommandsResponse
        {
            [JsonProperty("player")] public OnlineCommandsPlayer Player { get; set; }
            [JsonProperty("commands")] public List<Command> Commands { get; set; }
        }

        public class OnlineCommandsPlayer
        {
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("username")] public string Username { get; set; }
            [JsonProperty("meta")] public OnlineCommandPlayerMeta Meta { get; set; }
        }

        public class OnlineCommandPlayerMeta
        {
            [JsonProperty("avatar")] public string Avatar { get; set; } = "";
            [JsonProperty("avatarfull")] public string AvatarFull { get; set; } = "";
            [JsonProperty("steamID")] public string SteamId { get; set; } = "";
        }

        public class CommandConditions
        {
            [JsonProperty("delay")] public int Delay { get; set;  }
            [JsonProperty("slots")] public int Slots { get; set; }
        }

        public class OfflineCommandsMeta
        {
            [JsonProperty("limited")] public string Limited { get; set; }
        }
        public class OfflineCommandsResponse
        {
            [JsonProperty("meta")] public OfflineCommandsMeta Meta { get; set;  }
            [JsonProperty("commands")] public List<Command> Commands { get; set;  }
        }
        public class Command
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("command")] public string CommandToRun { get; set; } = "";
            [JsonProperty("payment")] public long Payment { get; set; }
            [JsonProperty(NullValueHandling=NullValueHandling.Ignore)] public long PackageRef { get; set; }
            [JsonProperty("conditions")] public CommandConditions Conditions { get; set; } = new CommandConditions();
            [JsonProperty("player")] public PlayerInfo Player { get; set; }
        }

        /**
             * List the players who have commands due to be executed when they next login to the game server.
             * This endpoint also returns any offline commands to be processed and the amount of seconds to wait before performing the queue check again.
             * All clients should strictly follow the response of `next_check`, failure to do so would result in your secret key being revoked or IP address being banned from accessing the API.
             */
        public void GetCommandQueue(ApiSuccessCallback onSuccess = null, ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            Send("queue", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        /**
             * Gets commands that can be executed on the player even if they are offline.
             */
        public void GetOfflineCommands(ApiSuccessCallback onSuccess = null, ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            Send($"queue/offline-commands", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        /**
             * Gets commands that can be executed for the given player if they are online.
             */
        public void GetOnlineCommands(int playerId, ApiSuccessCallback onSuccess, ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            Send($"queue/online-commands/{playerId}", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        private class DeleteCommandsPayload
        {
            /**
                 * An array of one or more command IDs to delete.
                 */
            [JsonProperty("ids")]
            public int[] Ids { get; set; }
        }

        /**
             * Deletes one or more commands that have been executed on the game server.
             * An empty response with the status code of 204 No Content will be returned on completion.
             */
        public void DeleteCommands(int[] ids, ApiSuccessCallback onSuccess, ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            var payload = new DeleteCommandsPayload
            {
                Ids = ids
            };
            Send("queue", JsonConvert.SerializeObject(payload), HttpVerb.DELETE, onSuccess, onApiError,
                onServerError);
        }

        #endregion

        #region Listing

        /**
             * Response from /listing containing the categories and their packages.
             */
        public class ListingsResponse
        {
            [JsonProperty("categories")] public List<Category> categories { get; set; }
        }

        /**
             * Get the categories and packages which should be displayed to players in game. The returned order of this endpoint
             * does not reflect the desired order of the category/packages - please order based on the order object.
             */
        public void GetListing(ApiSuccessCallback onSuccess = null, ApiErrorCallback onError = null,
            ServerErrorCallback onServerError = null)
        {
            Send("listing", "", HttpVerb.GET, onSuccess, onError, onServerError);
        }

        #endregion

        #region Packages

        /**
             * Get a list of all packages on the webstore. Pass verbose=true to include descriptions of the packages.
             * API returns a list of JSON encoded Packages.
             */
        public void GetAllPackages(bool verbose, ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            Send(verbose ? "packages?verbose=true" : "packages", "", HttpVerb.GET, onSuccess, onApiError,
                onServerError);
        }

        /**
             * Gets a specific package from the webstore by its ID. Returns JSON-encoded Package object.
             */
        public void GetPackage(string packageId, ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            Send($"package/{packageId}", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        /*
        // Updates a package on the webstore.
        public void UpdatePackage(string packageId, Package package, ApiSuccessCallback onSuccess = null, ApiErrorCallback onApiError = null, ServerErrorCallback onServerError = null)
        {
            //NOOP
        }
        */

        #endregion

        #region Community Goals

        // Retrieves all community goals from the account.
        public void GetCommunityGoals(ApiSuccessCallback onSuccess = null, ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            Send("community_goals", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        // Retrieves a specific community goal.
        public void GetCommunityGoal(int goalId, ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            Send($"community_goals/{goalId}", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        #endregion

        #region Payments

        /** Payload for /payments to retrieve all payments with quantity limit */
        public class PaymentsPayload
        {
            [JsonProperty("limit")] public int Limit { get; set; } = 100;
        }

        /**
             * Retrieve the latest payments (up to a maximum of 100) made on the webstore.
             */
        public void GetAllPayments(int limit = 100, ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null, ServerErrorCallback onServerError = null)
        {
            var payload = new PaymentsPayload
            {
                Limit = limit
            };

            if (limit > 100)
            {
                limit = 100;
            }

            Send($"payments", JsonConvert.SerializeObject(payload), HttpVerb.GET, onSuccess, onApiError,
                onServerError);
        }

        /**
             * Return all payments as a page, at the given page number.
             */
        public void GetAllPaymentsPaginated(int pageNumber, ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null, ServerErrorCallback onServerError = null)
        {
            Send($"payments?paged={pageNumber}", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        /**
             * Retrieve a specific payment by transaction id.
             */
        public void GetPayment(string transactionId, ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null, ServerErrorCallback onServerError = null)
        {
            Send($"payments/{transactionId}", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        /**
            // Returns an array of fields (custom variables, etc) required to be entered for a manual payment to be created for a package.
            public void GetRequiredPaymentFields(Package package)
            {
                //var response = client.SendAsyncRequest($"payments/fields/{package.Id}", HttpMethod.Get);
            }
            
              //Create a manual payment in the same way as is possible from the control panel. One or more packages should be added to the payment,
              //and the package commands will be processed in the same way as would be for a standard manual payment.
                public void CreatePayment()
                {
                    Send($"payments", HttpMethod.Post);
                }

                // Updates a payment
                public void UpdatePayment(string transactionId)
                {
                   Send($"payments/{transactionId}", HttpMethod.Put);
                }

                // Create a note against a payment.
                public void CreatePaymentNote(string transactionId, string note)
                {
                   Send($"payments/{transactionId}/note", HttpMethod.Post);
                }
            */

        #endregion

        #region Checkout

        private class CreateCheckoutPayload
        {
            [JsonProperty("package_id")] public int PackageId { get; set; }

            [JsonProperty("username")] public string Username { get; set; } = "";
        }

        /**
             * Creates a URL which will take the player to a checkout area in order to purchase the given item.
             */
        public void CreateCheckoutUrl(int packageId, string username, ApiSuccessCallback success,
            ApiErrorCallback error = null)
        {
            var payload = new CreateCheckoutPayload
            {
                PackageId = packageId,
                Username = username
            };

            Send("checkout", JsonConvert.SerializeObject(payload), HttpVerb.POST, success, error);
        }

        #endregion

        #region Gift Cards

        public class GiftCard
        {
            //TODO            
        }

        public void GetAllGiftCards(ApiSuccessCallback onSuccess = null, ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            Send("gift-cards", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        public void GetGiftCard(string giftCardId, ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null, ServerErrorCallback onServerError = null)
        {
            Send($"gift-cards/{giftCardId}", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        public class CreateGiftCardPayload
        {
            [JsonProperty("expires_at")] public string ExpiresAt { get; set; } = "";
            [JsonProperty("note")] public string Note { get; set; } = "";
            [JsonProperty("amount")] public double Amount { get; set; }
        }

        public void CreateGiftCard(string expiresAt, string note, int amount, ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null, ServerErrorCallback onServerError = null)
        {
            var payload = new CreateGiftCardPayload
            {
                ExpiresAt = expiresAt,
                Note = note,
                Amount = amount
            };
            Send("gift-cards", JsonConvert.SerializeObject(payload), HttpVerb.POST, onSuccess, onApiError,
                onServerError);
        }

        public void VoidGiftCard(string giftCardId, ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null, ServerErrorCallback onServerError = null)
        {
            Send($"gift-cards/{giftCardId}", "", HttpVerb.DELETE, onSuccess, onApiError, onServerError);
        }

        public class TopUpGiftCardPayload
        {
            [JsonProperty("amount")] public string Amount { get; set; } = "";
        }

        public void TopUpGiftCard(string giftCardId, double amount, ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null, ServerErrorCallback onServerError = null)
        {
            var payload = new TopUpGiftCardPayload
            {
                Amount = $"{amount}"
            };
            Send($"gift-cards/{giftCardId}", JsonConvert.SerializeObject(payload), HttpVerb.PUT);
        }

        #endregion

        #region Coupons

        public void GetAllCoupons(ApiSuccessCallback onSuccess = null, ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            Send("coupons", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        public void GetCouponById(string couponId, ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null, ServerErrorCallback onServerError = null)
        {
            Send($"coupons/{couponId}", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        /**
            public void CreateCoupon(ApiSuccessCallback onSuccess = null, ApiErrorCallback onApiError = null, ServerErrorCallback onServerError = null)
            {
                Send("coupons", HttpMethod.Post);
            }

            public void DeleteCoupon(string couponId, ApiSuccessCallback onSuccess = null, ApiErrorCallback onApiError = null, ServerErrorCallback onServerError = null)
            {
               Send($"gift-cards/{couponId}", HttpMethod.Delete);
            }*/

        #endregion

        #region Bans

        public void GetAllBans(ApiSuccessCallback onSuccess = null, ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            //var response = client.SendAsyncRequest("bans", HttpMethod.Get);
            Send("bans", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        public class CreateBanPayload
        {
            [JsonProperty("reason")] public string Reason { get; set; }
            [JsonProperty("ip")] public string IP { get; set; }

            /** Username or UUID of the player to ban */
            [JsonProperty("user")]
            public string User { get; set; }
        }

        public void CreateBan(string reason, string ip, string userId, ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null, ServerErrorCallback onServerError = null)
        {
            var payload = new CreateBanPayload
            {
                Reason = reason,
                IP = ip,
                User = userId
            };
            Send("bans", JsonConvert.SerializeObject(payload), HttpVerb.POST, onSuccess, onApiError,
                onServerError);
        }

        #endregion

        #region Sales

        public void GetAllSales(ApiSuccessCallback onSuccess = null, ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            Send("sales", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        #endregion

        #region Player Lookup

        /**
             * Root object returned by the /user endpoint, containing PlayerInfo
             */
        public class UserInfoResponse
        {
            [JsonProperty("player")] public PlayerInfo Player { get; set; }

            [JsonProperty("banCount")] public int BanCount { get; set; }

            [JsonProperty("chargebackRate")] public int ChargebackRate { get; set; }

            [JsonProperty("payments")] public List<PaymentInfo> Payments { get; set; }

            [JsonProperty("purchaseTotals")] public object[] PurchaseTotals { get; set; }
        }

        public class PaymentInfo
        {
            [JsonProperty("txn_id")] public string TransactionId { get; set; }

            [JsonProperty("time")] public long Time { get; set; }

            [JsonProperty("price")] public double Price { get; set; }

            [JsonProperty("currency")] public string Currency { get; set; }

            [JsonProperty("status")] public int Status { get; set; }
        }

        /**
             * A player's information returned by the /user endpoint
             */
        public class PlayerInfo
        {
            [JsonProperty("id")] public string Id { get; set; }

            //FIXME sometimes referred to as `name` or `username` alternatively?
            [JsonProperty("name")] public string Username { get; set; }

            [JsonProperty("meta")] public OnlineCommandPlayerMeta Meta { get; set; }

            /** Only populated by offline commands */
            [JsonProperty("uuid")]
            public string Uuid { get; set; } = "";
            
            [JsonProperty("plugin_username_id")] public int PluginUsernameId { get; set; }
        }

        public void GetUser(string targetUserId, ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null,
            ServerErrorCallback onServerError = null)
        {
            Send($"user/{targetUserId}", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
        }

        #endregion

        #region Customer Purchases

        public class PackagePurchaseInfo
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class CustomerPackagePurchaseRecord
        {
            [JsonProperty("txn_id")]
            public string TransactionId { get; set; }

            [JsonProperty("date")]
            public DateTime Date { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty("package")]
            public PackagePurchaseInfo Package { get; set; }
        }
        
        // Return a list of all active (non-expired) packages that a customer has purchased.
        // If packageId is provided, filter down to a single package ID, if you want to check if a specific package has been purchased. 
        public void GetActivePackagesForCustomer(string userId, int? packageId = null,
            ApiSuccessCallback onSuccess = null,
            ApiErrorCallback onApiError = null, ServerErrorCallback onServerError = null)
        {
            if (packageId == null)
            {
                Send($"player/{userId}/packages", "", HttpVerb.GET, onSuccess, onApiError, onServerError);
            }
            else
            {
                Send($"player/{userId}/packages?package={packageId}", "", HttpVerb.GET, onSuccess, onApiError,
                    onServerError);
            }
        }

        #endregion
    }
}