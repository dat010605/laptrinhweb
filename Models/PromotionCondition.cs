using System;

namespace FashionEcommerce.API.Models;

public partial class PromotionCondition
{
    public int PromotionConditionId { get; set; }
    public int? VoucherId { get; set; }
    public int? ProductId { get; set; } // optional constraint
    public decimal? MinOrderValue { get; set; }
    public int? MinQuantity { get; set; }
    public int Priority { get; set; } // higher = applied first

    public virtual Voucher? Voucher { get; set; }
    public virtual Product? Product { get; set; }
}