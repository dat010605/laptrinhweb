using System.ComponentModel.DataAnnotations;

namespace laptrinhweb.DTOs
{
    public class CartDto
    {
        [Required]
        public int UserId { get; set; } 

        [Required]
        public int VariantId { get; set; } 

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }
    }
}