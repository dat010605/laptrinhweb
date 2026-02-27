using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FashionEcommerce.API.Data;
using FashionEcommerce.API.Models;
using FashionEcommerce.API.DTOs;

namespace FashionEcommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc phải đăng nhập mới dùng được các API này
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- PHẦN 1: QUẢN LÝ THÔNG TIN CÁ NHÂN ---

        // GET: api/user/profile (Xem thông tin cá nhân của tôi)
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users
                .Select(u => new { u.UserId, u.Username, u.FullName, u.Email, u.PhoneNumber, u.Role })
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return user != null ? Ok(user) : NotFound("Không tìm thấy người dùng!");
        }

        // PUT: api/user/profile (Cập nhật họ tên, số điện thoại của tôi)
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FullName = request.FullName ?? user.FullName;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;

            await _context.SaveChangesAsync();
            return Ok("Cập nhật thông tin thành công!");
        }

        // --- PHẦN 2: QUẢN LÝ ĐỊA CHỈ (CRUD) ---

        // GET: api/user/addresses (Lấy toàn bộ địa chỉ của tôi)
        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var addresses = await _context.UserAddresses
                .Where(a => a.UserId == userId)
                .ToListAsync();
            return Ok(addresses);
        }

        // POST: api/user/addresses (Thêm địa chỉ mới)
        [HttpPost("addresses")]
        public async Task<IActionResult> AddAddress([FromBody] CreateAddressDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var newAddress = new UserAddress
            {
                UserId = userId,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                AddressLine = request.AddressLine,
                City = request.City,
                IsDefault = request.IsDefault
            };

            // Nếu đây là địa chỉ đầu tiên của người dùng, tự động đặt làm mặc định
            if (!await _context.UserAddresses.AnyAsync(a => a.UserId == userId))
                newAddress.IsDefault = true;

            _context.UserAddresses.Add(newAddress);
            await _context.SaveChangesAsync();

            return Ok("Thêm địa chỉ thành công!");
        }

        // PUT: api/user/addresses/{id} (Sửa địa chỉ)
        [HttpPut("addresses/{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateAddressDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Tìm địa chỉ và đảm bảo nó thuộc về người đang đăng nhập
            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);

            if (address == null) return NotFound("Không tìm thấy địa chỉ hoặc bạn không có quyền sửa!");

            // Cập nhật thông tin (nếu giá trị gửi lên không null)
            address.FullName = request.FullName ?? address.FullName;
            address.PhoneNumber = request.PhoneNumber ?? address.PhoneNumber;
            address.AddressLine = request.AddressLine ?? address.AddressLine;
            address.City = request.City ?? address.City;

            // Xử lý logic địa chỉ mặc định (Nếu đặt cái này làm Default, các cái khác phải thôi)
            if (request.IsDefault == true)
            {
                var otherAddresses = await _context.UserAddresses
                    .Where(a => a.UserId == userId && a.AddressId != id)
                    .ToListAsync();
                foreach (var item in otherAddresses) item.IsDefault = false;
                address.IsDefault = true;
            }

            await _context.SaveChangesAsync();
            return Ok("Cập nhật địa chỉ thành công!");
        }

        // DELETE: api/user/addresses/{id} (Xóa địa chỉ)
        [HttpDelete("addresses/{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);

            if (address == null) return NotFound("Không tìm thấy địa chỉ để xóa!");

            _context.UserAddresses.Remove(address);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa địa chỉ thành công!");
        }

        // --- PHẦN 3: DÀNH RIÊNG CHO ADMIN ---

        // GET: api/user/all (Admin xem danh sách tất cả tài khoản)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.UserId, u.Username, u.Email, u.Role, u.IsLocked })
                .ToListAsync();
            return Ok(users);
        }

        // PATCH: api/user/{id}/lock (Admin khóa hoặc mở khóa tài khoản)
        [HttpPatch("{id}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User không tồn tại!");
            
            // Không cho phép Admin tự khóa chính mình hoặc Admin khác qua API này để an toàn
            if (user.Role == "Admin") return BadRequest("Không thể khóa tài khoản Quản trị viên!");

            user.IsLocked = !user.IsLocked; // Đảo trạng thái (True <-> False)
            await _context.SaveChangesAsync();
            
            return Ok(new { Message = user.IsLocked ? "Đã khóa tài khoản thành công!" : "Đã mở khóa tài khoản thành công!" });
        }
    }
}