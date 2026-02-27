namespace FashionEcommerce.API.DTOs
{
    public class UpdateAddressDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public bool? IsDefault { get; set; }
    }
}