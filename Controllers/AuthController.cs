using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FashionEcommerce.API.Data;
using FashionEcommerce.API.Models;
using FashionEcommerce.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace FashionEcommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. API ĐĂNG KÝ (POST: api/auth/register)
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Tên đăng nhập đã tồn tại!");
                
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email đã được sử dụng!");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = "Customer" 
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("Đăng ký tài khoản thành công!");
        }

        // 2. API ĐĂNG NHẬP (POST: api/auth/login)
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Sai tên đăng nhập hoặc mật khẩu!");
            }

            var token = CreateToken(user);

            return Ok(new { Token = token, Message = "Đăng nhập thành công!" });
        }

        // 3. API ĐĂNG KÝ ADMIN (POST: api/auth/register-admin)
        [HttpPost("register-admin")]
        [Authorize(Roles = "Admin")] // CHỈ NGƯỜI CÓ QUYỀN ADMIN MỚI ĐƯỢC TẠO ADMIN KHÁC
        public async Task<IActionResult> RegisterAdmin(RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Tên đăng nhập này đã tồn tại!");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email này đã được sử dụng!");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var adminUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = "Admin" 
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Đã tạo thành công tài khoản Quản trị viên: {request.Username}" });
        }

        // HÀM HỖ TRỢ: Tạo Token JWT
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "Customer") 
            };

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1), 
                SigningCredentials = creds,
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}