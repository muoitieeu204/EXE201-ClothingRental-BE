using System;
using System.Collections.Generic;
using EXE201.Service.DTOs.ReviewImageDTOs;

namespace EXE201.Service.DTOs.ReviewDTOs
{
    public class ReviewResponseDto
    {
        public int ReviewId { get; set; }
        public int OutfitId { get; set; }
        public int UserId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<ReviewImageResponseDto> Images { get; set; } = new();
    }
}
