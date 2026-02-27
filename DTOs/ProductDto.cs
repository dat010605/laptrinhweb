namespace FashionEcommerce.API.DTOs
{
    // Khuôn hứng dữ liệu từ Admin khi thêm sản phẩm mới
    public class CreateProductRequestDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public int CategoryId { get; set; } // Phải chọn nó thuộc Danh mục nào

        // Danh sách link ảnh tải lên
        public List<string> ImageUrls { get; set; } = new List<string>();

        // Danh sách các biến thể (Màu, Size, Số lượng)
        public List<CreateVariantDto> Variants { get; set; } = new List<CreateVariantDto>();
    }

    // Khuôn hứng dữ liệu cho từng biến thể nhỏ
    public class CreateVariantDto
    {
        public string? Size { get; set; } // Cho phép null nếu là Balo
        public string? Color { get; set; } // Cho phép null
        public string Sku { get; set; } = null!; // Bắt buộc phải có mã kho (VD: VABA-DO-20)
        public decimal? PriceAdjustment { get; set; } // Giá cộng thêm so với BasePrice
        public int StockQuantity { get; set; } // SỐ LƯỢNG TỒN KHO
    }
}
// DTO để trả thông tin Sản phẩm ra ngoài
    public class ProductResponseDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; } // Lấy luôn tên danh mục cho tiện
        
        public List<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();
        public List<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
    }

    public class ProductImageDto
    {
        public string ImageUrl { get; set; } = null!;
    }

    public class ProductVariantDto
    {
        public int VariantId { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string Sku { get; set; } = null!;
        public decimal? PriceAdjustment { get; set; }
        public int? StockQuantity { get; set; }
    }
    // DTO dùng để nhận dữ liệu khi cần trừ kho
    public class DeductStockRequestDto
    {
        public int VariantId { get; set; } // ID của biến thể (Màu, Size) khách mua
        public int QuantityToDeduct { get; set; } // Số lượng khách mua
    }
    // DTO dùng để hứng dữ liệu khi Admin muốn sửa thông tin cơ bản của Sản phẩm
    public class UpdateProductDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public int CategoryId { get; set; }
        public bool IsActive { get; set; } // Cho phép Admin bật/tắt hiển thị sản phẩm
    }