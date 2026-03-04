namespace FashionEcommerce.API.DTOs
{
    public class ArticleDto
    {
        public string Title { get; set; } = null!;
        public string? Slug { get; set; }
        public string? Content { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int? AuthorId { get; set; }
        public bool? IsPublished { get; set; }
    }
}