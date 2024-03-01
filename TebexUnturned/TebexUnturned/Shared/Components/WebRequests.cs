using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Tebex.Adapters;
using Tebex.API;

namespace Tebex.Shared.Components
{
    public class WebRequests
    {
        private static BaseTebexAdapter _adapter;
        private static HttpClient _http;
        private static Queue<TebexRequest> _requestQueue;
        public WebRequests(BaseTebexAdapter adapter)
        {
            _adapter = adapter;
            _http = new HttpClient();
            _requestQueue = new Queue<TebexRequest>();
        }

        public void Enqueue(string url, string body, Action<int, string> callback, TebexApi.HttpVerb method = TebexApi.HttpVerb.GET, Dictionary<string, string> headers = null, float timeout = 0.0f)
        {
            _requestQueue.Enqueue(new TebexRequest(url, body, callback, method, headers, timeout));
        }

        public void Enqueue(TebexRequest request)
        {
            _requestQueue.Enqueue(request);
        }
        
        public async Task ProcessNextRequestAsync()
        {
            _requestQueue.Dequeue();
            
            if (_requestQueue.Count == 0) return;

            TebexRequest request = _requestQueue.Dequeue();

            try
            {
                _http.Timeout = TimeSpan.FromMilliseconds(request.Timeout * 1000);
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = new HttpMethod(request.Method.ToString()),
                    RequestUri = new Uri(request.Url),
                    Content = new StringContent(request.Body),
                };
                
                foreach (var header in request.Headers)
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value);
                }

                var response = await _http.SendAsync(httpRequestMessage);
                var responseBody = await response.Content.ReadAsStringAsync();
                var truncatedResponse = responseBody;
                if (truncatedResponse.Length > 256)
                {
                    truncatedResponse = truncatedResponse.Substring(0, 251) + "[...]";
                }
                
                var logInStr = $"{response.StatusCode} | '{truncatedResponse}' <- {request.Method.ToString()} {request.Url}";
                _adapter.LogDebug(logInStr);
                
                request.Callback?.Invoke((int)response.StatusCode, responseBody);
            }
            catch (Exception ex)
            {
                //TODO report via triage
                request.Callback?.Invoke(0, $"Request failed: {ex.Message}");
            }
        }
    }

    public class TebexRequest
    {
        public string Url { get; private set; }
        public string Body { get; private set; }
        public Action<int, string> Callback { get; private set; }
        public TebexApi.HttpVerb Method { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public float Timeout { get; private set; }

        public TebexRequest(string url, string body, Action<int, string> callback, TebexApi.HttpVerb method, Dictionary<string, string> headers, float timeout)
        {
            Url = url;
            Body = body;
            Callback = callback;
            Method = method;
            Headers = headers;
            Timeout = timeout;
        }
    }

}