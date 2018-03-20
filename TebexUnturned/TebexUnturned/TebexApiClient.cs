using System;
using System.Net;
using System.Threading.Tasks;
using Rocket.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TebexUnturned
{
    public class TebexApiClient : WebClient
    {

        //time in milliseconds
        private Tebex plugin;
        private int timeout;
        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }

        public void setPlugin(Tebex plugin)
        {
            this.plugin = plugin;
        }

        public TebexApiClient(int timeout = 5000)
        {
            this.timeout = timeout;
        }

        public void DoGet(string endpoint, ITebexCommand command)
        {
            this.Headers.Add("X-Buycraft-Secret", this.plugin.Configuration.Instance.secret);
            String url = this.plugin.Configuration.Instance.baseUrl + endpoint;
            Tebex.logWarning("GET " + url);
            this.DownloadStringCompleted += (sender, e) =>
            {
                if (!e.Cancelled && e.Error == null)
                {
                    command.HandleResponse(JObject.Parse(e.Result));    
                }
                else
                {
                    command.HandleError(e.Error);
                }
                this.Dispose();
            };
            this.DownloadStringAsync(new Uri(url));
        }
             
    }
}