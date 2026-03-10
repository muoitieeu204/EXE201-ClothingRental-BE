using System.Text.Json.Serialization;

namespace EXE201.Service.DTOs.ChatbotDTOs
{
    /// <summary>
    /// Criteria extracted by Gemini from user message to search outfits
    /// </summary>
    internal class OutfitSearchCriteria
    {
        [JsonPropertyName("queryType")]  public string QueryType        { get; set; } = string.Empty; // greeting | best_sellers | cheapest | most_expensive | search
        [JsonPropertyName("keywords")]   public List<string> Keywords   { get; set; } = new();
        [JsonPropertyName("type")]       public string Type             { get; set; } = string.Empty;
        [JsonPropertyName("gender")]     public string Gender           { get; set; } = string.Empty;
        [JsonPropertyName("region")]     public string Region           { get; set; } = string.Empty;
        [JsonPropertyName("occasion")]   public string Occasion         { get; set; } = string.Empty;
        [JsonPropertyName("priceRange")] public string PriceRange       { get; set; } = string.Empty;
    }
}
