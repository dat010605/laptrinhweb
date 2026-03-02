using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FashionEcommerce.API.Data; // Đảm bảo namespace DbContext đúng
using FashionEcommerce.API.Models;
using FashionEcommerce.API.DTOs;
using laptrinhweb.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context; 

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API 1: TẠO ĐƠN HÀNG (CHECKOUT)
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy giỏ hàng của User
                var cartItems = await _context.Carts.Where(c => c.UserId == dto.UserId).ToListAsync();
                if (!cartItems.Any()) return BadRequest("Giỏ hàng trống.");

                // 1. Tạo đơn hàng (Orders)
                var order = new Order
                {
                    UserId = dto.UserId,
                    
                    OrderDate = DateTime.Now,
                    OrderStatus = "Chờ xử lý",
                    ShippingAddress = dto.ShippingAddress
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Lưu để lấy order.OrderId

                foreach (var item in cartItems)
                {
                   
                    var variant = await _context.ProductVariants
                                                .Include(v => v.Product)
                                                .FirstOrDefaultAsync(v => v.VariantId == item.VariantId);

                    if (variant == null || variant.Product == null)
                    {
                        throw new Exception("Có lỗi với dữ liệu sản phẩm trong giỏ hàng.");
                    }

                    // 2. LƯU SNAPSHOT DATA
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId, // Lưu ý: Tên cột khóa chính của Order có thể là OrderId thay vì Id
                        ProductId = variant.Product.ProductId, // Lấy ID sản phẩm từ biến thể
                        ProductNameSnapshot = variant.Product.Name,
                        PriceSnapshot = variant.Product.BasePrice, 
                        Quantity = item.Quantity
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // 3. XỬ LÝ RACE CONDITION (Trừ kho ở bảng ProductVariants)
                   
                    var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
    $"UPDATE ProductVariants SET StockQuantity = StockQuantity - {item.Quantity} WHERE VariantId = {item.VariantId} AND StockQuantity >= {item.Quantity}");
                    
                    if (rowsAffected == 0)
                    {
                        throw new Exception($"Sản phẩm '{variant.Product.Name}' đã hết hàng hoặc không đủ số lượng.");
                    }
                }

                // 4. Xóa giỏ hàng sau khi mua xong
                _context.Carts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                
                // Trả về OrderId (kiểm tra lại tên khóa chính của bạn là Id hay OrderId)
                return Ok(new { Message = "Đặt hàng thành công", OrderId = order.OrderId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        // API 2: CẬP NHẬT TRẠNG THÁI & LƯU LỊCH SỬ
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] UpdateOrderStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound("Không tìm thấy đơn hàng.");

            // 1. Lưu lịch sử
            var history = new OrderStatusHistory
            {
                OrderId = orderId,
                OldStatus = order.OrderStatus,
                NewStatus = dto.NewStatus,
                UpdateDate = DateTime.Now
            };
            _context.OrderStatusHistories.Add(history);

            // 2. Cập nhật trạng thái
            order.OrderStatus = dto.NewStatus;
            
            await _context.SaveChangesAsync();

            return Ok("Cập nhật trạng thái thành công.");
        }
    }
}