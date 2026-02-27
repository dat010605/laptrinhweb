namespace FashionEcommerce.API.DTOs
{
    // DTO dùng để trả dữ liệu cho Frontend (có chứa danh mục con)
    public class CategoryResponseDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? Slug { get; set; }
        public int? ParentId { get; set; }
        public bool? IsVisible { get; set; }
        public List<CategoryResponseDto> SubCategories { get; set; } = new List<CategoryResponseDto>();
    }

    // DTO dùng để nhận dữ liệu từ Admin khi tạo mới danh mục
    public class CreateCategoryDto
    {
        public string CategoryName { get; set; } = null!;
        public string? Slug { get; set; }
        public int? ParentId { get; set; } // Nếu tạo danh mục cha thì để trống
        public bool? IsVisible { get; set; } = true;
    }
}