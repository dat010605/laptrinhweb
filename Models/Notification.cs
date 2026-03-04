using System;

namespace FashionEcommerce.API.Models;

public partial class Notification
{
    public int NotificationId { get; set; }
    public int? UserId { get; set; }
    public string Message { get; set; } = null!;
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
}