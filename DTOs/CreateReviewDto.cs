namespace FashionEcommerce.API.DTOs
{
    public class CreateReviewDto
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}