namespace EXE201.Service.DTOs.OutfitImageDTOs
{
    public class OutfitImageResponseDto
    {
        public int ImageId { get; set; }
        public int OutfitId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? ImageType { get; set; }
    public int? SortOrder { get; set; }
    }
}
