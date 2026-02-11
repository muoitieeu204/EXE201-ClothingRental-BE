using System.Collections.Generic;

namespace EXE201.Service.DTOs.ReviewDTOs
{
    public class UpdateReviewDto
    {
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public List<string>? ImageUrls { get; set; }
    }
}
