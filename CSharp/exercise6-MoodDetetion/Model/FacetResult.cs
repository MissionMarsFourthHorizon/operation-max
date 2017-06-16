namespace HelpDeskBot.Model
{
    using Newtonsoft.Json;

    public class FacetResult
    {
        [JsonProperty("@odata.context")]
        public string ODataContext { get; set; }

        [JsonProperty("@search.facets")]
        public SearchFacets Facets { get; set; }

        public SearchResultHit[] Value { get; set; }
    }
}