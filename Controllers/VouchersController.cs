using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FashionEcommerce.API.Data;
using FashionEcommerce.API.DTOs;
using FashionEcommerce.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // only administrators can manage vouchers
    public class VouchersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VouchersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/vouchers
        // Admin can retrieve all vouchers they have created (or all in system)
        [HttpGet]
        public async Task<IActionResult> GetAllVouchers()
        {
            var vouchers = await _context.Vouchers.ToListAsync();
            return Ok(vouchers);
        }

        // POST: api/vouchers
        // body: CreateVoucherDto
        [HttpPost]
        public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherDto request)
        {
            // check product exists
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
                return NotFound("Sản phẩm không tồn tại.");

            if (request.EndDate <= request.StartDate)
                return BadRequest("Ngày kết thúc phải lớn hơn ngày bắt đầu.");

            // prepare voucher entity
            var voucher = new Voucher
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

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Voucher đã được tạo.", voucher.VoucherId });
        }

        // other endpoints could be added later (e.g. apply voucher logic, etc.)
    }
}