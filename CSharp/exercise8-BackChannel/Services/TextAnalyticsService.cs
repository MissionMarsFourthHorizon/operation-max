namespace HelpDeskBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Configuration;
    using Newtonsoft.Json;

    [Serializable]
    public class TextAnalyticsService
    {
        public async Task<double> Sentiment(string text)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("https://westus.api.cognitive.microsoft.com/");
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", WebConfigurationManager.AppSettings["TextAnalyticsApiKey"]);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var sentimentRequest = new SentimentRequest()
                {
                    Documents = new List<SentimentDocument>()
                    {
                        new SentimentDocument(text)
                    }
                };

                string uri = "/text/analytics/v2.0/sentiment";

                var response = await httpClient.PostAsJsonAsync<SentimentRequest>(uri, sentimentRequest);
                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TextAnalyticsResult>(responseString);

                if (result.Documents.Length == 1)
                {
                    return result.Documents[0].Score;
                }

                return double.NaN;
            }
        }

        internal class TextAnalyticsResult
        {
            public TextAnalyticsResultDocument[] Documents { get; set; }
        }

        internal class TextAnalyticsResultDocument
        {
            public string Id { get; set; }

            public double Score { get; set; }
        }

        internal class SentimentRequest
        {
            public SentimentRequest()
            {
                this.Documents = new List<SentimentDocument>();
            }

            public List<SentimentDocument> Documents { get; set; }
        }

        internal class SentimentDocument
        {
            public SentimentDocument()
            {
                this.Language = "en";
                this.Id = "single";
            }

            public SentimentDocument(string text)
                : this()
            {
                this.Text = text;
            }

            public string Language { get; set; }

            public string Id { get; set; }

            public string Text { get; set; }
        }
    }
}