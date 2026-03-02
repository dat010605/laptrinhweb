using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionEcommerce.API.Data; 
using FashionEcommerce.API.Models;
using FashionEcommerce.API.DTOs;
using laptrinhweb.DTOs;
namespace laptrinhweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context; 

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartDto cartDto)
        {
            // 1. Kiểm tra kho từ bảng ProductVariant thay vì bảng Product
            var variant = await _context.ProductVariants.FindAsync(cartDto.VariantId);
            if (variant == null) return NotFound("Không tìm thấy phân loại sản phẩm này.");
            
            // LƯU Ý: Nếu trong file ProductVariant.cs của bạn, biến tồn kho không tên là 'Stock' mà tên là 'Quantity'
            // Hãy sửa chữ variant.Stock dưới đây thành variant.Quantity 
            if (variant.StockQuantity < cartDto.Quantity) 
            {
                return BadRequest("Sản phẩm không đủ hàng trong kho.");
            }

            // 2. Thêm vào giỏ với VariantId
            var cartItem = new Cart { 
                UserId = cartDto.UserId, 
                VariantId = cartDto.VariantId, 
                Quantity = cartDto.Quantity,
                CreatedDate = DateTime.Now     
            };
            
            _context.Carts.Add(cartItem);
            await _context.SaveChangesAsync();

            return Ok("Đã thêm vào giỏ hàng.");
        }
    }
}