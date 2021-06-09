using ProductSynchronizer.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace ProductSynchronizer.Helpers
{
    public class HttpRequestHelper
    {
        private Queue<(bool isProxy, HttpClient httpClient, string userAgent, string ip)> _httpClients = new Queue<(bool isProxy, HttpClient httpClient, string userAgent, string ip)>();
        public HttpRequestHelper(Resource resource, HttpClientVpnType vpnType = HttpClientVpnType.NoProxy)
        {
            if (vpnType != HttpClientVpnType.OnlyProxy)
                _httpClients.Enqueue((false, new HttpClient(), "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.120 Safari/537.36", "serverIP"));

            if (vpnType != HttpClientVpnType.NoProxy)
            {
                var proxies = ConfigHelper.Config.ProxiesConfig.Where(x => x.Resource.HasValue && x.Resource.Value == resource).ToList();
                if (!proxies.Any()){
                    proxies = ConfigHelper.Config.ProxiesConfig.Where(x => !x.Resource.HasValue).ToList();
                }

                foreach (var proxyConfig in proxies)
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

                    var client = new HttpClient(handler: httpClientHandler, disposeHandler: true);

                    _httpClients.Enqueue((true, client, proxyConfig.UserAgent, proxyConfig.ProxyIpPort));
                }
            }
        }
        public string PerformGetRequest(string url)
        {
            Log.WriteLog($"Get request for url: {url}");
            Thread.Sleep(_httpClients.Count <= 2 ? 40000 : 25000);

            while (true)
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var queueClient = _httpClients.Dequeue();
                    _httpClients.Enqueue(queueClient);

                    request.Headers.Add("User-Agent", queueClient.userAgent);
                    request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");

                    var response = queueClient.httpClient.SendAsync(request);
                    var result = response.Result;

                    Log.WriteLog($"Status code: {result.StatusCode}, for url: {url}");

                    if (result.StatusCode == HttpStatusCode.OK)
                        return result.Content.ReadAsStringAsync().Result;
                    
                    Log.WriteLog($"isProxy: [{queueClient.isProxy}], user-agent: [{queueClient.userAgent}], proxy ip: [{queueClient.ip}], response: [{result.Content?.ReadAsStringAsync().Result}]");
                    throw new Exception($"isProxy: [{queueClient.isProxy}], proxy ip: [{queueClient.ip}]");
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
    public enum HttpClientVpnType
    {
        NoProxy,
        MixProxy,
        OnlyProxy
    }
}
