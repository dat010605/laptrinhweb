using System;
using System.Collections.Generic;

namespace FashionEcommerce.API.Models;

public partial class Article
{
    public int ArticleId { get; set; }

    public string Title { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Content { get; set; }

    public string? ThumbnailUrl { get; set; }

    public int? AuthorId { get; set; }

    public bool? IsPublished { get; set; }

    public DateTime? PublishedAt { get; set; }

    public virtual User? Author { get; set; }
}
