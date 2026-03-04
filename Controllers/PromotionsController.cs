using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionEcommerce.API.Data;
using FashionEcommerce.API.DTOs;
using FashionEcommerce.API.Models;

namespace FashionEcommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PromotionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/promotions/calculate
        // body: ApplyVoucherDto
        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] ApplyVoucherDto request)
        {
            if (request.Codes == null || request.Codes.Count == 0)
                return BadRequest("Phải cung cấp ít nhất một mã.");

            // load vouchers matching provided codes
            var vouchers = await _context.Vouchers
                .Where(v => request.Codes.Contains(v.Code))
                .Include(v => v.PromotionConditions)
                .ToListAsync();

            if (!vouchers.Any())
            {
                return Ok(new PromotionResultDto { Message = "Không tìm thấy mã hợp lệ." });
            }

            // filter by conditions applicable to the product
            var applicable = vouchers
                .Select(v => new
                {
                    Voucher = v,
                    Condition = v.PromotionConditions
                        .Where(c => !c.ProductId.HasValue || c.ProductId == request.ProductId)
                        .OrderByDescending(c => c.Priority)
                        .FirstOrDefault()
                })
                .Where(x => x.Condition != null)
                .OrderByDescending(x => x.Condition?.Priority ?? 0)
                .FirstOrDefault();

            if (applicable == null)
            {
                // no condition matched, just pick highest priority voucher overall
                var fallback = vouchers
                    .OrderByDescending(v => v.PromotionConditions.Max(c => (int?)c.Priority) ?? 0)
                    .First();

                return Ok(new PromotionResultDto
                {
                    AppliedCode = fallback.Code,
                    DiscountValue = fallback.DiscountValue,
                    Message = "Không có điều kiện đặc biệt, áp dụng mã ưu tiên nhất."
                });
            }

            return Ok(new PromotionResultDto
            {
                AppliedCode = applicable.Voucher.Code,
                DiscountValue = applicable.Voucher.DiscountValue,
                Message = "Áp dụng mã theo điều kiện ưu tiên."
            });
        }
    }
}