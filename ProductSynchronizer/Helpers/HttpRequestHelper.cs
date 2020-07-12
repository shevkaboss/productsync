using ProductSynchronizer.Logger;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace ProductSynchronizer.Helpers
{
    public class HttpRequestHelper
    {
        private Queue<HttpClient> _httpClients = new Queue<HttpClient>();
        public HttpRequestHelper(bool withProxy = false)
        {
            _httpClients.Enqueue(new HttpClient());

            if (withProxy)
            {
                foreach (var proxyConfig in ConfigHelper.Config.ProxiesConfig)
                {
                    var proxy = new WebProxy
                    {
                        Address = new Uri($"http://{proxyConfig.ProxyIpPort}"),
                        BypassProxyOnLocal = false,
                        UseDefaultCredentials = false,

                        Credentials = new NetworkCredential(
                            proxyConfig.Login,
                            proxyConfig.Password
                        )
                    };

                    var httpClientHandler = new HttpClientHandler
                    {
                        Proxy = proxy,
                    };

                    //// Omit this part if you don't need to authenticate with the web server:
                    //if (needServerAuthentication)
                    //{
                    //    httpClientHandler.PreAuthenticate = true;
                    //    httpClientHandler.UseDefaultCredentials = false;

                    //    // *** These creds are given to the web server, not the proxy server ***
                    //    httpClientHandler.Credentials = new NetworkCredential(
                    //        userName: serverUserName,
                    //        password: serverPassword);
                    //}

                    var client = new HttpClient(handler: httpClientHandler, disposeHandler: true);

                    _httpClients.Enqueue(client);
                }
            }
        }
        public string PerformGetRequest(string url)
        {
            Log.WriteLog($"Get request for url: {url}");
            var retryCount = 0;

            while (true)
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.120 Safari/537.36");

                    var client = _httpClients.Dequeue();
                    _httpClients.Enqueue(client);

                    var response = client.SendAsync(request);
                    var result = response.Result;

                    Log.WriteLog($"Status code: {result.StatusCode}, for url: {url}");

                    if (result.StatusCode == HttpStatusCode.OK)
                        return result.Content.ReadAsStringAsync().Result;
                    else
                    {
                        if (retryCount == 0 && result.StatusCode == HttpStatusCode.Forbidden)
                        {
                            Thread.Sleep(15000);
                            Log.WriteLog($"Retrying Request.");
                            retryCount++;
                            continue;
                        }
                        return null;
                    }


                }
            }
        }

        public static string PerformGetRequestStatic(string url)
        {
            Log.WriteLog($"Get request for url: {url}");

            using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
            {
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.120 Safari/537.36");

                using (var client = new HttpClient())
                {
                    var response = client.SendAsync(request);
                    var result = response.Result;

                    Log.WriteLog($"Status code: {result.StatusCode}, for url: {url}");

                    if (result.StatusCode == HttpStatusCode.OK)
                        return result.Content.ReadAsStringAsync().Result;
                    return null;
                }
            }
        }

    }
}
