using System;
using System.Collections.Generic;

namespace FashionEcommerce.API.Models;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int? OrderId { get; set; }
    public int? ProductId { get; set; }

    public int? VariantId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public virtual Order? Order { get; set; }

    public virtual ProductVariant? Variant { get; set; }
    public string? ProductNameSnapshot { get; set; } 
    
        public decimal? PriceSnapshot { get; set; }
        
    
}
