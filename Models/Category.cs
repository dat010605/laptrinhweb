using System;
using System.Collections.Generic;

namespace FashionEcommerce.API.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Slug { get; set; }

    public int? ParentId { get; set; }

    public bool? IsVisible { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
