using System.ComponentModel.DataAnnotations;

namespace laptrinhweb.DTOs
{
    public class CheckoutDto
    {
        [Required(ErrorMessage = "Vui lòng cung cấp mã người dùng.")]
        public int UserId { get; set; } 

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
        public string ShippingAddress { get; set; }

        // có thể mở rộng thêm các trường khác nếu cần thiết cho dự án:
        // public string PhoneNumber { get; set; }
        // public string PaymentMethod { get; set; } 
        // public string Note { get; set; } // Ghi chú của khách hàng
    }
}