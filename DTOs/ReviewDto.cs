using System.ComponentModel.DataAnnotations;

namespace FashionEcommerce.API.DTOs
{
    public class CreateReviewDto
    {
        [Required]
        public int ProductId { get; set; }
        
        public int? VariantId { get; set; } // Có thể đánh giá cụ thể 1 biến thể (màu sắc/size) nếu muốn

        [Required]
        [Range(1, 5, ErrorMessage = "Số sao đánh giá phải từ 1 đến 5.")]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}