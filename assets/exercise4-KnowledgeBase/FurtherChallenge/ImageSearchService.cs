namespace HelpDeskBot.Services
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    [Serializable]
    public class ImageSearchService
    {
        private static Random random = new Random();

        public async Task<string> GetImage(string text)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("https://api.cognitive.microsoft.com/bing/v5.0/images/search");
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", WebConfigurationManager.AppSettings["ImageSearchApiKey"]);

                string uri = $"?q={text}&count=10&safeSearch=Strict&imageType=Clipart";

                var response = await httpClient.PostAsync(uri, null);
                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ImageResult>(responseString);

                var index = random.Next(9);

                if (result?.Value.Count == 10)
                {
                    return result.Value[index].ThumbnailUrl;
                }
                return null;
            }
        }

        internal class ImageResult
        {
            public IList<ImageResultItem> Value { get; set; }
        }

        internal class ImageResultItem
        {
            public string ThumbnailUrl { get; set; }
        }
    }
}