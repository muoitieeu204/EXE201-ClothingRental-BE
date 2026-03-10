namespace EXE201.Service.DTOs.ChatbotDTOs
{
    public class ChatMessageResponseDto
    {
        public string Response { get; set; } = string.Empty;
        public List<OutfitSuggestionDto>? OutfitSuggestions { get; set; }
    }

    public class OutfitSuggestionDto
    {
        public int OutfitId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? RentalPrice { get; set; }
        public string? Type { get; set; }
        public string? Gender { get; set; }
        public string? Region { get; set; }
        public string? ImageUrl { get; set; }
    }
}
