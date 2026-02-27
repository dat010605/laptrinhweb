using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FashionEcommerce.API.Data;
using FashionEcommerce.API.Models;
using FashionEcommerce.API.DTOs; // Thêm dòng này để gọi được thư mục DTOs

namespace FashionEcommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bất kỳ ai ĐÃ ĐĂNG NHẬP (Customer hay Admin) đều được dùng
    public class AddressesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AddressesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: Khách hàng thêm địa chỉ giao hàng mới
        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody] AddressDto request) // ĐỔI SANG NHẬN DTO NGẮN GỌN
        {
            // 1. Lấy Id của người dùng ĐANG ĐĂNG NHẬP từ trong Token ra
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) 
                return Unauthorized("Không xác định được danh tính. Vui lòng đăng nhập lại!");

            int currentUserId = int.Parse(userIdString);
            
            // 2. Chuyển đổi dữ liệu từ hộp chứa DTO sang Model Address thực tế
            var newAddress = new Address
            {
                UserId = currentUserId,
                RecipientName = request.RecipientName,
                PhoneNumber = request.PhoneNumber,
                Street = request.Street,
                Ward = request.Ward,
                District = request.District,
                City = request.City,
                IsDefault = request.IsDefault
            };

            // 3. LOGIC QUAN TRỌNG: XỬ LÝ ĐỊA CHỈ MẶC ĐỊNH
            // Nếu khách hàng tick chọn địa chỉ mới này là "Mặc định"
            if (newAddress.IsDefault == true)
            {
                // Tìm TẤT CẢ các địa chỉ cũ của khách hàng này đang là mặc định
                var oldDefaultAddresses = await _context.Addresses
                    .Where(a => a.UserId == currentUserId && a.IsDefault == true)
                    .ToListAsync();

                // Chuyển toàn bộ các địa chỉ cũ đó về trạng thái false (Không mặc định)
                foreach (var oldAddress in oldDefaultAddresses)
                {
                    oldAddress.IsDefault = false;
                }
            }

            // 4. Lưu địa chỉ mới vào Database
            _context.Addresses.Add(newAddress);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đã thêm địa chỉ giao hàng thành công!" });
        }
    }
}