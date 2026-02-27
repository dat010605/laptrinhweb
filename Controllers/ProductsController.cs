using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionEcommerce.API.Data; 
using FashionEcommerce.API.Models;
using FashionEcommerce.API.DTOs;

namespace FashionEcommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: Thêm Sản phẩm mới KÈM Hình ảnh và Biến thể (Kho)
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequestDto request)
        {
            // 1. Tạo thực thể Sản phẩm
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                BasePrice = request.BasePrice,
                CategoryId = request.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // 2. Gắn danh sách Ảnh vào sản phẩm
            foreach (var url in request.ImageUrls)
            {
                product.ProductImages.Add(new ProductImage 
                { 
                    ImageUrl = url
                    // Có thể bổ sung logic chọn ảnh đầu tiên làm ảnh chính (IsMain = true)
                });
            }

            // 3. Gắn danh sách Biến thể (Size, Màu, Tồn kho) vào sản phẩm
            foreach (var v in request.Variants)
            {
                product.ProductVariants.Add(new ProductVariant
                {
                    Size = v.Size,
                    Color = v.Color,
                    Sku = v.Sku,
                    PriceAdjustment = v.PriceAdjustment,
                    StockQuantity = v.StockQuantity
                });
            }

            // 4. Lưu toàn bộ vào DB trong 1 nhịp
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Thêm sản phẩm, ảnh và nhập kho thành công!", ProductId = product.ProductId });
        }
        // API: Lấy danh sách Sản phẩm & Tìm kiếm
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts(
            [FromQuery] string? keyword, 
            [FromQuery] int? categoryId)
        {
            // 1. Tạo câu truy vấn gốc (Lấy Sản phẩm + Danh mục + Ảnh + Biến thể)
            // Chỉ lấy những sản phẩm đang được bán (IsActive == true)
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .Where(p => p.IsActive == true)
                .AsQueryable();

            // 2. Logic Tìm kiếm (Nếu có người dùng gõ từ khóa)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                // Tìm tương đối trong Tên hoặc Mô tả
                query = query.Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword));
            }

            // 3. Logic Lọc theo Danh mục (VD: Khách bấm vào mục "Vali")
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // 4. Thực thi truy vấn và đóng gói vào DTO
            var products = await query.Select(p => new ProductResponseDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                BasePrice = p.BasePrice,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.CategoryName : null,
                Images = p.ProductImages.Select(i => new ProductImageDto
                {
                    ImageUrl = i.ImageUrl
                }).ToList(),
                Variants = p.ProductVariants.Select(v => new ProductVariantDto
                {
                    VariantId = v.VariantId,
                    Size = v.Size,
                    Color = v.Color,
                    Sku = v.Sku,
                    PriceAdjustment = v.PriceAdjustment,
                    StockQuantity = v.StockQuantity
                }).ToList()
            }).ToListAsync();

            return Ok(products);
        }
        // API: Trừ số lượng tồn kho (Dành cho Thành viên 3 gọi khi tạo đơn hàng)
        [HttpPut("deduct-stock")]
        public async Task<IActionResult> DeductStock([FromBody] List<DeductStockRequestDto> requests)
        {
            // Mở một Transaction (Giao dịch) để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in requests)
                {
                    // 1. Tìm mặt hàng trong kho
                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(v => v.VariantId == item.VariantId);

                    // 2. Kiểm tra xem mặt hàng có tồn tại không
                    if (variant == null)
                    {
                        return NotFound(new { Message = $"Không tìm thấy sản phẩm với VariantId {item.VariantId}" });
                    }

                    // 3. Kiểm tra xem kho có đủ hàng để bán không
                    if (variant.StockQuantity < item.QuantityToDeduct)
                    {
                        return BadRequest(new { 
                            Message = $"Sản phẩm mã {variant.Sku} không đủ hàng! Kho chỉ còn {variant.StockQuantity} cái." 
                        });
                    }

                    // 4. Thực hiện trừ kho
                    variant.StockQuantity -= item.QuantityToDeduct;
                }

                // Lưu tất cả thay đổi xuống SQL Server
                await _context.SaveChangesAsync();
                
                // Xác nhận giao dịch thành công (Commit)
                await transaction.CommitAsync();

                return Ok(new { Message = "Trừ kho thành công cho toàn bộ đơn hàng!" });
            }
            catch (Exception ex)
            {
                // Nếu có bất kỳ lỗi gì xảy ra, hoàn tác lại toàn bộ (Rollback)
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "Lỗi hệ thống khi cập nhật kho", Error = ex.Message });
            }
        }
        // API: Cập nhật thông tin cơ bản của Sản phẩm
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto request)
        {
            // Tìm sản phẩm theo ID
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { Message = "Không tìm thấy sản phẩm này!" });
            }

            // Cập nhật các trường thông tin
            product.Name = request.Name;
            product.Description = request.Description;
            product.BasePrice = request.BasePrice;
            product.CategoryId = request.CategoryId;
            product.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cập nhật thông tin sản phẩm thành công!" });
        }

        // API: Xóa Mềm (Ẩn) Sản phẩm
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { Message = "Không tìm thấy sản phẩm này!" });
            }

            // Xóa mềm: Chuyển IsActive thành false để ẩn khỏi cửa hàng
            product.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đã ẩn sản phẩm thành công (Không xóa mất lịch sử đơn hàng)!" });
        }
        // API: Lấy chi tiết 1 sản phẩm theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto>> GetProductById(int id)
        {
            var p = await _context.Products
                .Include(prod => prod.Category)
                .Include(prod => prod.ProductImages)
                .Include(prod => prod.ProductVariants)
                .FirstOrDefaultAsync(prod => prod.ProductId == id && prod.IsActive == true);

            if (p == null)
            {
                return NotFound(new { Message = "Không tìm thấy sản phẩm này!" });
            }

            var result = new ProductResponseDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                BasePrice = p.BasePrice,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName,
                Images = p.ProductImages.Select(i => new ProductImageDto { ImageUrl = i.ImageUrl }).ToList(),
                Variants = p.ProductVariants.Select(v => new ProductVariantDto
                {
                    VariantId = v.VariantId,
                    Size = v.Size,
                    Color = v.Color,
                    Sku = v.Sku,
                    PriceAdjustment = v.PriceAdjustment,
                    StockQuantity = v.StockQuantity
                }).ToList()
            };

            return Ok(result);
        }
        // API: Tải ảnh vật lý lên Server
        [HttpPost("upload-images")]
        public async Task<IActionResult> UploadImages(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(new { Message = "Ngài chưa chọn file ảnh nào!" });
            }

            var imageUrls = new List<string>();
            
            // Đường dẫn tới thư mục lưu ảnh: wwwroot/images/products
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

            // Nếu thư mục chưa tồn tại thì tự động tạo mới
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // Tạo tên file độc nhất bằng Guid để không bao giờ bị trùng tên
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Copy file vật lý vào ổ cứng server
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    // Lưu lại đường dẫn tương đối (để frontend dùng hiển thị)
                    imageUrls.Add($"/images/products/{uniqueFileName}");
                }
            }

            return Ok(new { Message = "Tải ảnh thành công!", Urls = imageUrls });
        }
    }
}