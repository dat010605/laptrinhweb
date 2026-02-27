using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionEcommerce.API.Data; 
using FashionEcommerce.API.Models;
using FashionEcommerce.API.DTOs;

namespace FashionEcommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. API: Lấy toàn bộ cây Danh mục (Vali -> Vali Nhựa, Balo -> Balo Laptop...)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
        {
            // Chỉ lấy những danh mục GỐC (ParentId == null) và đính kèm luôn danh mục con của nó
            var categories = await _context.Categories
                .Where(c => c.ParentId == null)
                .Include(c => c.SubCategories)
                .Select(c => new CategoryResponseDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Slug = c.Slug,
                    ParentId = c.ParentId,
                    IsVisible = c.IsVisible,
                    SubCategories = c.SubCategories.Select(sub => new CategoryResponseDto
                    {
                        CategoryId = sub.CategoryId,
                        CategoryName = sub.CategoryName,
                        Slug = sub.Slug,
                        ParentId = sub.ParentId,
                        IsVisible = sub.IsVisible
                    }).ToList()
                })
                .ToListAsync();

            return Ok(categories);
        }

        // 2. API: Admin tạo Danh mục mới
        [HttpPost]
        public async Task<ActionResult<CategoryResponseDto>> CreateCategory([FromBody] CreateCategoryDto request)
        {
            var category = new Category
            {
                CategoryName = request.CategoryName,
                Slug = request.Slug,
                ParentId = request.ParentId,
                IsVisible = request.IsVisible
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategories), new { id = category.CategoryId }, request);
        }
    }
}