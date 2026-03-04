namespace FashionEcommerce.API.DTOs
{
    public class ApplyVoucherDto
    {
        public int ProductId { get; set; }
        public List<string> Codes { get; set; } = new List<string>();
    }
}