using System.Net.Http;

namespace ProductSynchronizer.Helpers
{
    public static class HttpRequestHelper
    {
        public static string PerformGetRequest(string url)
        {
            var request = new HttpRequestMessage(new HttpMethod("GET"), url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.120 Safari/537.36");
            using (var webClient = new HttpClient())
            {
                var response = webClient.SendAsync(request);
                return response.Result.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
