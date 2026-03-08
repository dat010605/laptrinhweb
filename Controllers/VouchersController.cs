using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionEcommerce.API.Data;
using FashionEcommerce.API.Models;
using FashionEcommerce.API.DTOs;

namespace FashionEcommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VouchersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VouchersController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherDto request)
        {
            // Kiểm tra xem mã này đã từng được tạo chưa
            if (await _context.Vouchers.AnyAsync(v => v.Code == request.Code.ToUpper()))
                return BadRequest("Mã giảm giá này đã tồn tại trong hệ thống!");

            var voucher = new Voucher
            {
                Code = request.Code.ToUpper(),
                Description = request.Description,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MinOrderValue = request.MinOrderValue,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                UsageLimit = request.UsageLimit,
                IsActive = true
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đã tạo mã giảm giá thành công!" });
        }

        
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherDto request)
        {
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == request.Code.ToUpper() && v.IsActive == true);

           
            if (voucher == null) return NotFound("Mã giảm giá không tồn tại hoặc đã bị khóa!");

            if (voucher.UsageLimit <= 0) return BadRequest("Mã giảm giá này đã hết lượt sử dụng!");

          
            if (voucher.StartDate.HasValue && voucher.StartDate > DateTime.Now)
                return BadRequest("Mã giảm giá này chưa đến thời gian áp dụng!");

          
            if (voucher.EndDate.HasValue && voucher.EndDate < DateTime.Now)
                return BadRequest("Mã giảm giá này đã hết hạn!");

       
            if (voucher.MinOrderValue.HasValue && request.OrderTotal < voucher.MinOrderValue.Value)
                return BadRequest($"Đơn hàng của bạn phải từ {voucher.MinOrderValue.Value:N0}đ để áp dụng mã này!");

           
            
            decimal discountAmount = 0;

            if (voucher.DiscountType == "Percent") 
            {
                discountAmount = request.OrderTotal * (voucher.DiscountValue / 100);
            }
            else 
            {
                discountAmount = voucher.DiscountValue;
            }

           
            if (discountAmount > request.OrderTotal) discountAmount = request.OrderTotal;

            decimal finalTotal = request.OrderTotal - discountAmount;

            return Ok(new 
            { 
                Message = "Áp dụng mã thành công!",
                DiscountAmount = discountAmount,
                FinalTotal = finalTotal
            });
        }
    }
}