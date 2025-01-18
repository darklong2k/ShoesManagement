using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Areas.Admin.Models;
using Shoes_Management.Models;
using System.Text.RegularExpressions;

namespace Shoes_Management.Areas.API
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class ProductAPIController : ControllerBase
    {
        private readonly Shoescontext _context;

        public ProductAPIController(Shoescontext context)
        {
            _context = context;
        }
        // Hàm tạo slug
        private string GenerateSlug(string name)
        {
            var slug = Regex.Replace(name.ToLower(), @"[^a-z0-9]+", "-").Trim('-');
            return slug;
        }
        [HttpGet("GetAll")]
        public IActionResult GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var productsQuery = _context.Products.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    productsQuery = productsQuery.Where(c =>
                        c.Name.Contains(search) || c.Description.Contains(search));
                }

                var totalItems = productsQuery.Count();
                var products = productsQuery
                                        .GroupJoin(_context.ProductDetails
                                        .GroupBy(pd => pd.ProductId)
                                        .Select(g => new
                                         {
                                             ProductId = g.Key,
                                             TotalQuantity = g.Sum(pd => pd.StockQuantity) // Tính tổng số lượng
                                         }),
                                         p => p.ProductId,
                                         pd => pd.ProductId,
                                         (p, productDetails) => new
                                         {
                                             p.ProductId,
                                             p.Name,
                                             p.Price,
                                             p.Description,
                                             Stock = productDetails.Select(pd => pd.TotalQuantity).FirstOrDefault() != 0
                                                     ? productDetails.Select(pd => pd.TotalQuantity).FirstOrDefault()
                                                     : 0, // Gán 0 nếu không có dữ liệu
                                             p.RatingAvg,
                                             p.Status
                                         }
                                     )
                                     // Sắp xếp để đảm bảo thứ tự
                                     .OrderBy(p=>p.Status)
     
                                     .Skip((page - 1) * pageSize) // Phân trang
                                     .Take(pageSize) // Phân trang   
                                     .ToList();


                return Ok(new
                {
                    totalItems,
                    products,
                    totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/ProductAPI/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] Product model)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (!ModelState.IsValid)
                    return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ." });
                //Kiểm tra tên sản phẩm không vượt quá 255 ký tự
                if (model.Name.Length > 255)
                {
                    return BadRequest(new { Success = false, Message = "Tên sản phẩm không được vượt quá 255 ký tự." });
                }
                // Kiểm tra tên sản phẩm trùng lặp
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name.ToLower() == model.Name.ToLower());
                if (existingProduct != null)
                {
                    return BadRequest(new { Success = false, Message = "Tên sản phẩm đã tồn tại. Vui lòng nhập tên khác." });
                }

                // Tạo slug cho sản phẩm
                var slug = GenerateSlug(model.Name);

                // Giả sử model.ImageFile là IFormFile để nhận file hình ảnh
                string imageFileName = null;
                if (model.ImageFile != null)
                {
                    // Đặt đường dẫn lưu hình ảnh vào thư mục wwwroot/images
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", model.ImageFile.FileName);

                    // Lưu tệp vào thư mục
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);  // Lưu tệp
                    }

                    // Lưu tên tệp vào cơ sở dữ liệu
                    imageFileName = model.ImageFile.FileName;
                }

                // Tạo sản phẩm mới
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    BrandId = model.BrandId,
                    SupplierId = model.SupplierId,
                    CreatedAt = DateTime.Now,
                    RatingAvg = 5,
                    Outstanding = false,
                    Likes = 0,
                    Views = 0,
                    UpdatedAt = DateTime.Now,
                    Slug = $"{slug}-{Guid.NewGuid()}", // Thêm mã sản phẩm vào cuối slug
                    Image = imageFileName,
                    Status = "Active" // Mặc định sản phẩm có trạng thái hoạt động
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Thêm sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, new { Success = false, Message = "Lỗi hệ thống: " + ex.Message });
            }
            }
        // GET: api/ProductAPI/Get/{id}
        [HttpGet("Get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { Success = false, Message = "Product not found." });

            return Ok(new { Success = true, Data = product });
        }
        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromForm] Product model)
        {
            try
            {
                
                // Kiểm tra dữ liệu đầu vào
                if (!ModelState.IsValid)
                    return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ." });

                // Tìm sản phẩm cần cập nhật theo ID
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == model.ProductId);

                if (existingProduct == null)
                {
                    return NotFound(new { Success = false, Message = "Sản phẩm không tồn tại." });
                }

                // Kiểm tra tên sản phẩm trùng lặp (trừ sản phẩm hiện tại)
                var duplicateProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name.ToLower() == model.Name.ToLower() && p.ProductId != model.ProductId);

                if (duplicateProduct != null)
                {
                    return BadRequest(new { Success = false, Message = "Tên sản phẩm đã tồn tại. Vui lòng nhập tên khác." });
                }
                // Cập nhật hình ảnh nếu có thay đổi
                if (model.ImageFile != null)
                {
                    // Xóa ảnh cũ (nếu có) để tránh lưu trữ thừa
                    if (!string.IsNullOrEmpty(existingProduct.Image))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", existingProduct.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Lưu hình ảnh mới
                    var newImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", model.ImageFile.FileName);
                    using (var stream = new FileStream(newImagePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }
                    existingProduct.Image = model.ImageFile.FileName;
                }
                // Cập nhật thông tin sản phẩm
                existingProduct.Name = model.Name;
                existingProduct.Description = model.Description;
                existingProduct.Price = model.Price;
                existingProduct.CategoryId = model.CategoryId;
                existingProduct.BrandId = model.BrandId;
                existingProduct.SupplierId = model.SupplierId;
                existingProduct.UpdatedAt = DateTime.Now;
                existingProduct.Status = model.Status ?? existingProduct.Status;  // Nếu không có trạng thái, giữ nguyên

                

                // Cập nhật sản phẩm vào cơ sở dữ liệu
                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Cập nhật sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, new { Success = false, Message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        // DELETE: api/ProductAPI/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { Success = false, Message = "Product not found." });
            product.Status = "Inactive";

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Product deleted successfully." });

        }
    }
}
