using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.AddressDTOs
{
    public class CreateAddressDto
    {
        [MaxLength(50)]
        public string? Label { get; set; }

        [MaxLength(100)]
        public string? RecipientName { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        // Cái này nên bắt buộc để đặt hàng/ship
        [Required]
        [MaxLength(255)]
        public string AddressLine { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Ward { get; set; }

        [MaxLength(100)]
        public string? District { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        public bool? IsDefault { get; set; } // giữ bool? để khớp DB hiện tại (BIT có thể NULL)
    }
}
