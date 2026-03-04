using System;
using System.Collections.Generic;

namespace FashionEcommerce.API.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal BasePrice { get; set; }

    public int? CategoryId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<PromotionCondition> PromotionConditions { get; set; } = new List<PromotionCondition>();

    // vouchers that apply to this product
    public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
}
