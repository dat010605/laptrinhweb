using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FashionEcommerce.API.Data;
using FashionEcommerce.API.DTOs;
using FashionEcommerce.API.Models;

namespace FashionEcommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/notifications -> list for current user
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetForUser()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
            int userId = int.Parse(userIdString);
            var notes = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();
            return Ok(notes);
        }

        // POST: api/notifications (admin creates)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] NotificationDto dto)
        {
            var note = new Notification
            {
                UserId = dto.UserId,
                Message = dto.Message,
                Link = dto.Link,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(note);
            await _context.SaveChangesAsync();
            return Ok(note);
        }

        // POST: api/notifications/{id}/read
        [HttpPost("{id}/read")]
        [Authorize]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
            int userId = int.Parse(userIdString);
            var note = await _context.Notifications.FindAsync(id);
            if (note == null || note.UserId != userId) return NotFound();
            note.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(note);
        }
    }
}