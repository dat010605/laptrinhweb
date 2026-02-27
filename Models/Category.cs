using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionEcommerce.API.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Slug { get; set; }

    public int? ParentId { get; set; }

    public bool? IsVisible { get; set; }

[ForeignKey("ParentId")]
    // ĐỂ XỬ LÝ ĐỆ QUY CHA - CON:
    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
