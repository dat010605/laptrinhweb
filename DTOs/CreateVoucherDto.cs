using System;

namespace FashionEcommerce.API.DTOs
{
    public class CreateVoucherDto
    {
        public int ProductId { get; set; }

        // discount percentage (e.g. 10 means 10%)
        public decimal DiscountPercentage { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // optional fields
        public string? Code { get; set; }
        public string? Description { get; set; }
    }
}