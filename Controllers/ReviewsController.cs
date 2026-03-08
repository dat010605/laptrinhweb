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
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

      
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Select(r => new {
                    r.ReviewId,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    CustomerName = r.User != null ? r.User.FullName : "Khách hàng",
                    Avatar = r.User != null ? r.User.AvatarUrl : null
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(reviews);
        }

      
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto request)
        {
          
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var hasBoughtProduct = await _context.Orders
                .Where(o => o.UserId == userId && o.OrderStatus == "Completed")
                .AnyAsync(o => o.OrderDetails.Any(od => od.Variant != null && od.Variant.ProductId == request.ProductId));

            if (!hasBoughtProduct)
            {
                return BadRequest("Bạn chưa mua sản phẩm này hoặc đơn hàng chưa hoàn thành nên không thể đánh giá!");
            }

           
            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == request.ProductId);

            if (alreadyReviewed)
            {
                return BadRequest("Bạn đã đánh giá sản phẩm này rồi!");
            }

          
            var newReview = new Review
            {
                UserId = userId,
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(newReview);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cảm ơn bạn đã đánh giá sản phẩm!" });
        }
    }
}