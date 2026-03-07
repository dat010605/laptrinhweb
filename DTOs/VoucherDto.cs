namespace FashionEcommerce.API.DTOs
{
    // Dành cho Admin tạo mã mới
    public class CreateVoucherDto
    {
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Loại giảm giá: "Fixed" (Giảm thẳng tiền) hoặc "Percent" (Giảm theo %)
        public string DiscountType { get; set; } = "Fixed"; 
        
        public decimal DiscountValue { get; set; }
        public decimal MinOrderValue { get; set; } = 0;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int UsageLimit { get; set; } = 100;
    }

    // Dành cho Khách hàng khi họ nhập mã ở giỏ hàng
    public class ApplyVoucherDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; } // Tổng tiền giỏ hàng hiện tại để hệ thống đối chiếu
    }
}