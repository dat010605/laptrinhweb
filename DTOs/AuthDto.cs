namespace FashionEcommerce.API.DTOs
{
    // Hộp chứa dữ liệu khi khách hàng Đăng ký
    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } 
    }

    // Hộp chứa dữ liệu khi khách hàng Đăng nhập
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}