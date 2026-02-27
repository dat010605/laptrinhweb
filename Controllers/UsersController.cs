using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionEcommerce.API.Data;
using FashionEcommerce.API.Models;

namespace FashionEcommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Lớp khiên thép: CHỈ ADMIN mới được gọi các API trong file này
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. API: Xem danh sách toàn bộ khách hàng và nhân viên
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            // Chỉ lấy các thông tin cần thiết, KHÔNG lấy PasswordHash ra ngoài để bảo mật
            var users = await _context.Users
                .Select(u => new { u.UserId, u.Username, u.FullName, u.Email, u.Role, u.IsLocked })
                .ToListAsync();
            
            return Ok(users);
        }

        // 2. API: Phân quyền (Nâng cấp khách hàng lên Admin hoặc ngược lại)
        [HttpPut("{id}/change-role")]
        public async Task<IActionResult> ChangeRole(int id, [FromBody] string newRole)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Không tìm thấy người dùng!");

            user.Role = newRole;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Đã thay đổi quyền của {user.Username} thành {newRole}" });
        }

        // 3. API: Khóa / Mở khóa tài khoản
        [HttpPut("{id}/toggle-lock")]
        public async Task<IActionResult> ToggleLockUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Không tìm thấy người dùng!");

            // Đảo ngược trạng thái: Đang khóa thì mở, đang mở thì khóa
            user.IsLocked = !user.IsLocked; 
            await _context.SaveChangesAsync();

            string status = user.IsLocked ? "đã bị khóa" : "đã được mở khóa";
            return Ok(new { Message = $"Tài khoản {user.Username} {status}!" });
        }
    }
}