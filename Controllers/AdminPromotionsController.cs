using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionEcommerce.API.Data;
using FashionEcommerce.API.DTOs;
using FashionEcommerce.API.Models;

namespace FashionEcommerce.API.Controllers
{
    [Route("api/admin/promotions")]
    [ApiController]
    [Authorize(Roles = "Admin")] // chỉ admin mới quản lý khuyến mãi
    public class AdminPromotionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminPromotionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/admin/promotions
        [HttpGet]
        public async Task<IActionResult> GetPromotions()
        {
            // tạm thời dùng bảng Voucher làm "promotion" vì data model chưa tách riêng
            var promotions = await _context.Vouchers.ToListAsync();
            return Ok(promotions);
        }

        // POST: api/admin/promotions
        // body: CreateVoucherDto (giống với tạo voucher)
        [HttpPost]
        public async Task<IActionResult> CreatePromotion([FromBody] CreateVoucherDto request)
        {
            // validate thông tin tương tự như VouchersController
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
                return NotFound("Sản phẩm không tồn tại.");

            if (request.EndDate <= request.StartDate)
                return BadRequest("Ngày kết thúc phải lớn hơn ngày bắt đầu.");

            var promotion = new Voucher
            {
                ProductId = request.ProductId,
                Code = string.IsNullOrWhiteSpace(request.Code)
                    ? Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()
                    : request.Code,
                Description = request.Description,
                DiscountType = "Percentage",
                DiscountValue = request.DiscountPercentage,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true
            };

            _context.Vouchers.Add(promotion);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Chương trình khuyến mãi đã được tạo.", promotion.VoucherId });
        }
    }
}