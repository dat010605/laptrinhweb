using System.ComponentModel.DataAnnotations;

namespace FashionEcommerce.API.Models
{
    public class UserAddress
    {
        [Key]
        public int AddressId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsDefault { get; set; }

        // Liên kết ngược lại với bảng User
        public User? User { get; set; }
    }
}