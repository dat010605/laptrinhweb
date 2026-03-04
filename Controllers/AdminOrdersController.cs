using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionEcommerce.API.Data;

namespace FashionEcommerce.API.Controllers
{
    [Route("api/admin/orders")]
    [ApiController]
    [Authorize(Roles = "Admin")] // chỉ admin mới xem được thống kê
    public class AdminOrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/admin/orders/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var orders = await _context.Orders.ToListAsync();
            var totalOrders = orders.Count;
            var totalRevenue = orders.Sum(o => o.TotalAmount);

            var ordersByStatus = orders
                .GroupBy(o => o.OrderStatus)
                .Select(g => new { Status = g.Key ?? "Unknown", Count = g.Count() })
                .ToList();

            return Ok(new
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                OrdersByStatus = ordersByStatus
            });
        }
    }
}