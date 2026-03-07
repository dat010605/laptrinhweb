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

        // 1. POST: api/vouchers (ADMIN: Tạo mã giảm giá mới)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherDto request)
        {
            // Kiểm tra xem mã này đã từng được tạo chưa
            if (await _context.Vouchers.AnyAsync(v => v.Code == request.Code.ToUpper()))
                return BadRequest("Mã giảm giá này đã tồn tại trong hệ thống!");

            var voucher = new Voucher
            {
                Code = request.Code.ToUpper(), // Luôn lưu chữ IN HOA cho dễ quản lý
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

        // 2. POST: api/vouchers/apply (KHÁCH HÀNG: Áp dụng tính toán giảm giá)
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherDto request)
        {
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == request.Code.ToUpper() && v.IsActive == true);

            // BÀI TEST 1: Tồn tại và còn hoạt động
            if (voucher == null) return NotFound("Mã giảm giá không tồn tại hoặc đã bị khóa!");

            // BÀI TEST 2: Còn lượt dùng không?
            if (voucher.UsageLimit <= 0) return BadRequest("Mã giảm giá này đã hết lượt sử dụng!");

            // BÀI TEST 3: Đã đến ngày chưa?
            if (voucher.StartDate.HasValue && voucher.StartDate > DateTime.Now)
                return BadRequest("Mã giảm giá này chưa đến thời gian áp dụng!");

            // BÀI TEST 4: Đã quá hạn chưa?
            if (voucher.EndDate.HasValue && voucher.EndDate < DateTime.Now)
                return BadRequest("Mã giảm giá này đã hết hạn!");

            // BÀI TEST 5: Đơn hàng đủ điều kiện tối thiểu không?
            if (voucher.MinOrderValue.HasValue && request.OrderTotal < voucher.MinOrderValue.Value)
                return BadRequest($"Đơn hàng của bạn phải từ {voucher.MinOrderValue.Value:N0}đ để áp dụng mã này!");

            // ==========================================
            // VƯỢT QUA 5 BÀI TEST -> TIẾN HÀNH TÍNH TIỀN
            // ==========================================
            
            decimal discountAmount = 0;

            if (voucher.DiscountType == "Percent") // Nếu là giảm theo phần trăm
            {
                discountAmount = request.OrderTotal * (voucher.DiscountValue / 100);
            }
            else // Nếu là "Fixed" (giảm số tiền cố định)
            {
                discountAmount = voucher.DiscountValue;
            }

            // Đảm bảo tiền giảm không vượt quá tổng tiền đơn hàng (VD: Đơn 50k không thể áp mã giảm 100k thành âm tiền)
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