namespace FashionEcommerce.API.DTOs
{
    // Hộp chứa này chỉ yêu cầu người dùng nhập đúng những thứ này
    public class AddressDto
    {
        public string RecipientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}