using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Tebex.Adapters;
using Tebex.API;

namespace Tebex.Shared.Components
{
    public class WebRequests
    {
        private static BaseTebexAdapter _adapter;
        private static Queue<TebexRequest> _requestQueue;

        public WebRequests(BaseTebexAdapter adapter)
        {
            _adapter = adapter;
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

        public int GetNumQueuedRequests()
        {
            return _requestQueue.Count;
        }
        
        public async Task ProcessNextRequestAsync()
        {
            if (_requestQueue.Count == 0) return;

            TebexRequest request = _requestQueue.Dequeue();

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(request.Url);
                webRequest.Method = request.Method.ToString();
                webRequest.Timeout = (int)(request.Timeout * 1000);

                foreach (var header in request.Headers)
                {
                    webRequest.Headers[header.Key] = header.Value;
                }

                if (!string.IsNullOrEmpty(request.Body) && (request.Method == TebexApi.HttpVerb.POST ||
                                                            request.Method == TebexApi.HttpVerb.PUT || request.Method == TebexApi.HttpVerb.DELETE))
                {
                    webRequest.ContentType = "application/json";
                    using (Stream stream = await Task.Factory.FromAsync(webRequest.BeginGetRequestStream,
                               webRequest.EndGetRequestStream, null))
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        var logOutStr = $"-> {request.Method.ToString()} {request.Url} | {request.Body}";

                        _adapter.LogDebug(logOutStr); // Write the full output entry to a debug log
                        if (logOutStr.Length >
                            256) // Limit any sent size of an output string to 256 characters, to prevent sending too much data
                        {
                            logOutStr = logOutStr.Substring(0, 251) + "[...]";
                        }

                        await writer.WriteAsync(request.Body);
                    }
                }

                using (WebResponse response =
                       await Task.Factory.FromAsync(webRequest.BeginGetResponse, webRequest.EndGetResponse, null))
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string responseBody = await reader.ReadToEndAsync();
                    var truncatedResponse = responseBody.Length > 256
                        ? responseBody.Substring(0, 251) + "[...]"
                        : responseBody;

                    var logInStr =
                        $"{((HttpWebResponse)response).StatusCode} | '{truncatedResponse}' <- {request.Method.ToString()} {request.Url}";
                    _adapter.LogDebug(logInStr);

                    request.Callback?.Invoke((int)((HttpWebResponse)response).StatusCode, responseBody);
                }
            }
            catch (WebException webEx)
            {
                using (Stream responseStream = webEx.Response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string responseBody = await reader.ReadToEndAsync();
                    var truncatedResponse = responseBody.Length > 256
                        ? responseBody.Substring(0, 251) + "[...]"
                        : responseBody;

                    var logInStr =
                        $"{((HttpWebResponse)webEx.Response).StatusCode} | '{truncatedResponse}' <- {request.Method.ToString()} {request.Url}";
                    _adapter.LogDebug(logInStr);

                    request.Callback?.Invoke((int)((HttpWebResponse)webEx.Response).StatusCode, responseBody);
                }
            }
            catch (Exception ex)
            {
                _adapter.LogDebug($"Error sending request {request.Url}: " + ex.Message);
                _adapter.LogDebug($"- Exception: " + ex.ToString());
                _adapter.LogDebug($"- Type " + ex.GetType().ToString());
                _adapter.LogDebug($"- Request body " + request.Body);
                request.Callback?.Invoke(0, request.Body);
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
            Headers = headers ?? new Dictionary<string, string>();
            Timeout = timeout;
        }
    }
}
