using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FashionEcommerce.API.Data;
using FashionEcommerce.API.Models;
using FashionEcommerce.API.DTOs;

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
            // Kiểm tra xem Username hoặc Email đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Tên đăng nhập đã tồn tại!");
                
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email đã được sử dụng!");

            // Mã hóa mật khẩu bằng BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Tạo tài khoản mới
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash, // Lưu mật khẩu đã mã hóa
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = "Customer" // Mặc định ai đăng ký cũng là Khách hàng
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("Đăng ký tài khoản thành công!");
        }

        // 2. API ĐĂNG NHẬP (POST: api/auth/login)
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            // Tìm user trong database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            
            // Nếu không thấy user hoặc mật khẩu giải mã không khớp
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Sai tên đăng nhập hoặc mật khẩu!");
            }

            // Nếu đúng, tạo ra Token JWT để cấp cho người dùng
            var token = CreateToken(user);

            return Ok(new { Token = token, Message = "Đăng nhập thành công!" });
        }

        // HÀM HỖ TRỢ: Tạo Token JWT
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "Customer") // Đưa quyền (Role) vào token
            };

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1), // Token có hạn 1 ngày
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