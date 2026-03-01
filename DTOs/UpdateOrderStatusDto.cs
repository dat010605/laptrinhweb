using System.ComponentModel.DataAnnotations;

namespace FashionEcommerce.API.DTOs 
{
    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Vui lòng cung cấp trạng thái mới.")]
        public string NewStatus { get; set; } = null!;
    }
}