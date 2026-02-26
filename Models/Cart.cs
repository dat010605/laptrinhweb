using System;
using System.Collections.Generic;

namespace FashionEcommerce.API.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int? UserId { get; set; }

    public int? VariantId { get; set; }

    public int Quantity { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual User? User { get; set; }

    public virtual ProductVariant? Variant { get; set; }
}
