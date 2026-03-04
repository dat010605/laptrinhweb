using System;
using System.Collections.Generic;

namespace FashionEcommerce.API.Models;

public partial class Voucher
{
    public int VoucherId { get; set; }

    // foreign key to the product that this voucher applies to (nullable in case a generic voucher is supported)
    public int? ProductId { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public string? DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal? MinOrderValue { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? UsageLimit { get; set; }

    public bool? IsActive { get; set; }

    // navigation property linking to product
    public virtual Product? Product { get; set; }

    // conditions which determine applicability and priority
    public virtual ICollection<PromotionCondition> PromotionConditions { get; set; } = new List<PromotionCondition>();
}
