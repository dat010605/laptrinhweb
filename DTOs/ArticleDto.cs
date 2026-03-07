namespace FashionEcommerce.API.DTOs
{
    // DTO dùng khi Admin tạo bài viết mới
    public class CreateArticleDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool IsPublished { get; set; }
    }

    // DTO dùng khi Admin cập nhật bài viết
    public class UpdateArticleDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool? IsPublished { get; set; }
    }
}