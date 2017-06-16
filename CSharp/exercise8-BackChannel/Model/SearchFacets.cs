namespace HelpDeskBot.Model
{
    using Newtonsoft.Json;

    public class SearchFacets
    {
        [JsonProperty("category@odata.type")]
        public string CategoryOdataType { get; set; }

        public Category[] Category { get; set; }
    }
}