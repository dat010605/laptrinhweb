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
    public class ArticlesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ArticlesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/articles (Dành cho mọi người - Chỉ xem bài đã xuất bản)
        [HttpGet]
        public async Task<IActionResult> GetPublishedArticles()
        {
            var articles = await _context.Articles
                .Where(a => a.IsPublished == true)
                .Select(a => new {
                    a.ArticleId,
                    a.Title,
                    a.Slug,
                    a.ThumbnailUrl,
                    a.PublishedAt,
                    AuthorName = a.Author != null ? a.Author.FullName : "Admin"
                })
                .OrderByDescending(a => a.PublishedAt)
                .ToListAsync();

            return Ok(articles);
        }

        // 2. GET: api/articles/{id} (Xem chi tiết 1 bài viết)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticleById(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.ArticleId == id && a.IsPublished == true);

            if (article == null) return NotFound("Không tìm thấy bài viết hoặc bài viết chưa được xuất bản!");

            return Ok(new {
                article.ArticleId,
                article.Title,
                article.Content,
                article.ThumbnailUrl,
                article.PublishedAt,
                AuthorName = article.Author?.FullName
            });
        }

        // ==========================================
        // KHU VỰC DÀNH RIÊNG CHO ADMIN
        // ==========================================

        // 3. POST: api/articles (Admin thêm bài viết mới)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateArticle([FromBody] CreateArticleDto request)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Tạo Slug (đường dẫn thân thiện) từ Tiêu đề (VD: "Tin Tức 1" -> "tin-tuc-1")
            var slug = request.Title.ToLower().Replace(" ", "-"); 

            var newArticle = new Article
            {
                Title = request.Title,
                Slug = slug,
                Content = request.Content,
                ThumbnailUrl = request.ThumbnailUrl,
                AuthorId = adminId,
                IsPublished = request.IsPublished,
                PublishedAt = request.IsPublished ? DateTime.Now : null
            };

            _context.Articles.Add(newArticle);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đã tạo bài viết thành công!", ArticleId = newArticle.ArticleId });
        }

        // 4. PUT: api/articles/{id} (Admin sửa bài viết)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] UpdateArticleDto request)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound("Không tìm thấy bài viết!");

            article.Title = request.Title ?? article.Title;
            article.Content = request.Content ?? article.Content;
            article.ThumbnailUrl = request.ThumbnailUrl ?? article.ThumbnailUrl;

            // Cập nhật ngày xuất bản nếu chuyển trạng thái từ Nháp -> Xuất bản
            if (request.IsPublished.HasValue)
            {
                if (request.IsPublished == true && article.IsPublished == false)
                    article.PublishedAt = DateTime.Now;
                    
                article.IsPublished = request.IsPublished.Value;
            }

            // Cập nhật lại Slug nếu Title bị đổi
            if (request.Title != null)
                article.Slug = request.Title.ToLower().Replace(" ", "-");

            await _context.SaveChangesAsync();
            return Ok("Cập nhật bài viết thành công!");
        }

        // 5. DELETE: api/articles/{id} (Admin xóa bài viết)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound("Không tìm thấy bài viết để xóa!");

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa bài viết thành công!");
        }
    }
}