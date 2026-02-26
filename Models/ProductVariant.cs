using System;
using System.Collections.Generic;

namespace FashionEcommerce.API.Models;

public partial class ProductVariant
{
    public int VariantId { get; set; }

    public int? ProductId { get; set; }

    public string? Size { get; set; }

    public string? Color { get; set; }

    public string Sku { get; set; } = null!;

    public decimal? PriceAdjustment { get; set; }

    public int? StockQuantity { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Product? Product { get; set; }
}
