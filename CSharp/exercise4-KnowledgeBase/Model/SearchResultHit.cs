namespace HelpDeskBot.Model
{
    using Newtonsoft.Json;

    public class SearchResultHit
    {
        [JsonProperty("@search.score")]
        public float SearchScore { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public string Text { get; set; }
    }
}