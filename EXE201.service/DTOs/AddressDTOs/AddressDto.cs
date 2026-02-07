namespace EXE201.Service.DTOs.AddressDTOs
{
    public class AddressDto
    {
        public int AddressId { get; set; }
        public int UserId { get; set; }

        public string? Label { get; set; }
        public string? RecipientName { get; set; }
        public string? PhoneNumber { get; set; }

        public string? AddressLine { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }

        public bool? IsDefault { get; set; }
    }
}
