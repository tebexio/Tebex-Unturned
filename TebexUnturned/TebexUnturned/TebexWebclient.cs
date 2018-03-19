

using System;
using System.Json;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace TebexUnturned
{
    public class TebexWebclient : System.Net.Http.HttpClient
    {

        private Tebex plugin;

        public TebexWebclient(Tebex plugin)
        {
            this.plugin = plugin;
            this.DefaultRequestHeaders.Add("X-Buycraft-Secret", this.plugin.Configuration.Instance.secret);
        }        
        
        public async void Get(String endpoint, Action<JsonValue> callback)
        {
            Tebex.logWarning("GET " + endpoint);
            var response = await this.GetAsync(plugin.Configuration.Instance.baseUrl + endpoint);
            response.EnsureSuccessStatusCode();
            String content = await response.Content.ReadAsStringAsync();
            Tebex.logWarning("Response received... parse as json");
            var json = await Task.Run(() => JsonObject.Parse(content));
            callback(json);
        }
        
        
        public async void Post(String endpoint, JsonObject data, Action<JsonValue> callback)
        {
            Tebex.logWarning("POST " + endpoint);
            String postData = data.ToString();
            var response = await this.PostAsync(plugin.Configuration.Instance.baseUrl + endpoint, new StringContent(postData));
            response.EnsureSuccessStatusCode();
            String content = await response.Content.ReadAsStringAsync();
            Tebex.logWarning("Response received... parse as json");
            var json = await Task.Run(() => JsonObject.Parse(content));
            callback(json);
        }        
    }
}