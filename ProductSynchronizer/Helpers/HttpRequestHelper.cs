using ProductSynchronizer.Logger;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
            Log.WriteLog($"Get request for url: {url}");
            var retryCount = 0;

            while (true)
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.120 Safari/537.36");
                    //request.Headers.Add("accept-language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7,uk;q=0.6");

                    

                    using (var webClient = new HttpClient())
                    {
                        var response = webClient.SendAsync(request);
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
        }
    }
}
