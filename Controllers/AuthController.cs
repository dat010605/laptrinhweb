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
using Google.Apis.Auth; 

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
                Role = "Customer",
                IsLocked = false // Mặc định tài khoản mới không bị khóa
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("Đăng ký tài khoản thành công!");
        }

        // 2. API ĐĂNG NHẬP THƯỜNG (POST: api/auth/login)
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            
            // Bước 1: Kiểm tra tài khoản và mật khẩu
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Sai tên đăng nhập hoặc mật khẩu!");
            }

            // Bước 2: KIỂM TRA KHÓA (Quan trọng nhất)
            if (user.IsLocked) 
            {
                return BadRequest("Tài khoản của bạn đã bị khóa bởi Admin!");
            }

            // Bước 3: Tạo Token nếu mọi thứ hợp lệ
            var token = CreateToken(user);

            return Ok(new { Token = token, Message = "Đăng nhập thành công!" });
        }

        // 3. API ĐĂNG KÝ ADMIN (POST: api/auth/register-admin)
        [HttpPost("register-admin")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được tạo Admin khác
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
                Role = "Admin",
                IsLocked = false
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Đã tạo thành công tài khoản Quản trị viên: {request.Username}" });
        }

        // 4. API ĐĂNG NHẬP BẰNG GOOGLE
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto request)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { "560051899088-0a9b7ah3tikp4mtom73srfhi888hg29c.apps.googleusercontent.com" } 
                };
                
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

                // Nếu chưa có tài khoản thì tự động tạo mới
                if (user == null)
                {
                    user = new User
                    {
                        Username = payload.Email,
                        Email = payload.Email,
                        FullName = payload.Name,
                        Role = "Customer",
                        // Tạo mật khẩu ngẫu nhiên cho tài khoản Google (họ sẽ không dùng mật khẩu này để login)
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                        IsLocked = false
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                // KIỂM TRA KHÓA cho luồng Google
                if (user.IsLocked) 
                {
                    return BadRequest("Tài khoản của bạn đã bị khóa bởi Admin!");
                }

                var token = CreateToken(user);

                return Ok(new { Token = token, Message = "Đăng nhập Google thành công!" });
            }
            catch (InvalidJwtException)
            {
                return BadRequest("Token của Google không hợp lệ hoặc đã hết hạn.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // HÀM HỖ TRỢ: Tạo Token JWT (Dùng chung)
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