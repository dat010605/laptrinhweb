using System;
using System.ComponentModel.DataAnnotations;

namespace FashionEcommerce.API.Models
{
    public class OrderStatusHistory
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string? OldStatus { get; set; }
        public string NewStatus { get; set; } = null!;
        public DateTime UpdateDate { get; set; }
    }
}