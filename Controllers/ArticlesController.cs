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
    public class ArticlesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ArticlesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/articles
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var list = await _context.Articles.ToListAsync();
            return Ok(list);
        }

        // GET: api/articles/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();
            return Ok(article);
        }

        // POST: api/articles
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] ArticleDto dto)
        {
            var article = new Article
            {
                Title = dto.Title,
                Slug = dto.Slug,
                Content = dto.Content,
                ThumbnailUrl = dto.ThumbnailUrl,
                AuthorId = dto.AuthorId,
                IsPublished = dto.IsPublished,
                PublishedAt = dto.IsPublished == true ? DateTime.UtcNow : null
            };
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return Ok(article);
        }

        // PUT: api/articles/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ArticleDto dto)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();
            article.Title = dto.Title;
            article.Slug = dto.Slug;
            article.Content = dto.Content;
            article.ThumbnailUrl = dto.ThumbnailUrl;
            article.AuthorId = dto.AuthorId;
            if (dto.IsPublished == true && article.PublishedAt == null)
                article.PublishedAt = DateTime.UtcNow;
            article.IsPublished = dto.IsPublished;
            await _context.SaveChangesAsync();
            return Ok(article);
        }

        // DELETE: api/articles/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}