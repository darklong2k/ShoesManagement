using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Models;
using System.Text.RegularExpressions;

namespace Shoes_Management.Areas.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryAPIController : ControllerBase
    {

        private readonly Shoescontext _context;

        public CategoryAPIController(Shoescontext context)
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
                var categoriesQuery = _context.Categories.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    categoriesQuery = categoriesQuery.Where(c =>
                        c.Name.Contains(search) || c.Description.Contains(search));
                }

                var totalItems = categoriesQuery.Count();
                var categories = categoriesQuery
                    .OrderByDescending(c => c.Status)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new
                    {
                        c.CategoryId,
                        c.Name,
                        c.Description,
                        c.ParentId,
                        c.Status,
                        c.CreatedAt,
                        c.UpdatedAt
                    })
                    
                    .ToList();

                return Ok(new { totalItems, categories });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // GET: api/CategoryAPI/Get/{id}
        [HttpGet("Get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { Success = false, Message = "Category not found." });

            return Ok(new { Success = true, Data = category });
        }

        // POST: api/CategoryAPI/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] Category model)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (!ModelState.IsValid)
                    return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ." });
                // Kiểm tra tên danh mục trùng lặp
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == model.Name.ToLower());
                if (existingCategory != null)
                {
                    return BadRequest(new { Success = false, Message = "Tên danh mục đã tồn tại. Vui lòng nhập tên khác."    });
                }
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;
                model.Status = true;
                if(model.ParentId==0) model.ParentId = null;
                model.slug = GenerateSlug(model.Name);
                _context.Categories.Add(model);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Thêm danh mục thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, new { Success = false, Message = "Lỗi hệ thống: " + ex.Message });
            }
        }


        // PUT: api/Category/Edit

        [HttpPut("Edit")]
        public async Task<IActionResult> Edit([FromBody] Category model)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (!ModelState.IsValid)
                    return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ." });

                var category = await _context.Categories.FindAsync(model.CategoryId);
                if (category == null)
                    return NotFound(new { Success = false, Message = "Không tìm thấy danh mục." });

                // Kiểm tra tên danh mục trùng lặp
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == model.Name.ToLower() && c.CategoryId != model.CategoryId);
                if (existingCategory != null)
                {
                    return BadRequest(new { Success = false, Message = "Tên danh mục đã tồn tại. Vui lòng nhập tên khác." });
                }

                // Cập nhật thông tin danh mục
                category.Name = model.Name;
                if (model.ParentId == 0) category.ParentId = null;
                else category.ParentId = model.ParentId;
                category.Description = model.Description;

                category.Status = model.Status;
                category.UpdatedAt = DateTime.Now;

                // Cập nhật slug nếu tên thay đổi
                if (category.Name != model.Name)
                {
                    category.slug = GenerateSlug(model.Name);
                }

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Cập nhật danh mục thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, new { Success = false, Message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // DELETE: api/Category/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { Success = false, Message = "Category not found." });
            category.Status = false;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Category deleted successfully." });

        }
    }
}
