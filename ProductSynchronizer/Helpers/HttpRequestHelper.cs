using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace ProductSynchronizer.Helpers
{
    public static class HttpRequestHelper
    {
        private static readonly CookieContainer cookies = new CookieContainer();
        private static readonly HttpClientHandler handler = new HttpClientHandler
        {
            CookieContainer = cookies
        };
        public static string PerformGetRequest(string url)
        {
            Logger.Logger.WriteLog($"Get request for url: {url}");

            var request = new HttpRequestMessage(new HttpMethod("GET"), url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.120 Safari/537.36");
            request.Headers.Add("accept-language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7,uk;q=0.6");

            using (var webClient = new HttpClient())
            {
                var response = webClient.SendAsync(request);
                var result = response.Result;

                Logger.Logger.WriteLog($"Status code: {result.StatusCode} for url: {url}");

                if (result.StatusCode != HttpStatusCode.OK)
                    return result.Content.ReadAsStringAsync().Result;
                Logger.Logger.WriteLog($"Status code: {result.StatusCode}, for url: {url}");

                return null;
            }
        }
    }
}
