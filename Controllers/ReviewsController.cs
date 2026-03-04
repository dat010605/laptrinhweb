using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FashionEcommerce.API.Data;
using FashionEcommerce.API.DTOs;
using FashionEcommerce.API.Models;

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

        // GET: api/reviews?productId=123
        [HttpGet]
        public async Task<IActionResult> GetReviews([FromQuery] int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();
            return Ok(reviews);
        }

        // POST: api/reviews
        // only signed-in users can post reviews
        [HttpPost]
        [Authorize] 
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto request)
        {
            // get current user id
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();
            int userId = int.Parse(userIdString);

            // ensure user has at least one completed order containing this product/variant
            var hasCompleted = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Variant)
                .Where(od => od.Order != null && od.Order.UserId == userId && od.Order.OrderStatus == "Completed")
                .Where(od => od.Variant != null && ((od.Variant.ProductId == request.ProductId) || (request.VariantId.HasValue && od.VariantId == request.VariantId)))
                .AnyAsync();

            if (!hasCompleted)
                return BadRequest("Bạn chỉ có thể đánh giá khi đã hoàn thành đơn hàng chứa sản phẩm này.");

            var review = new Review
            {
                UserId = userId,
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(review);
        }
    }
}