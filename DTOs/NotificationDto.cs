namespace FashionEcommerce.API.DTOs
{
    public class NotificationDto
    {
        public int? UserId { get; set; }
        public string Message { get; set; } = null!;
        public string? Link { get; set; }
    }
}