using System;
using System.Collections.Generic;

namespace FashionEcommerce.API.Models;

public partial class Address
{
    public int AddressId { get; set; }

    public int? UserId { get; set; }

    public string RecipientName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? Street { get; set; }

    public string? Ward { get; set; }

    public string? District { get; set; }

    public string? City { get; set; }

    public bool? IsDefault { get; set; }

    public virtual User? User { get; set; }
}
