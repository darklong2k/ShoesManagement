using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Areas.Admin.Services;
using Shoes_Management.Models;

namespace Shoes_Management.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryAPIController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly Shoescontext _context;

        public CategoryAPIController(ICategoryService categoryService, Shoescontext context)
        {
            _categoryService = categoryService;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Truy vấn dữ liệu bất đồng bộ từ database
                var categories = await _context.Categories.ToListAsync();

                // Kiểm tra nếu danh sách rỗng
                if (!categories.Any())
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Không có danh mục sản phẩm nào."
                    });
                }

                // Trả về danh sách danh mục
                return Ok(new
                {
                    Success = true,
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về thông báo lỗi
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình xử lý.",
                    Error = "Lỗi máy chủ."  // Tránh việc trả về thông báo chi tiết lỗi từ ex.Message
                });
            }
        }

        // POST: api/Category/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] Category model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });

            var result = await _categoryService.AddCategoryAsync(model);
            if (result)
                return Ok(new { message = "Thêm danh mục thành công" });

            return StatusCode(500, new { message = "Thêm danh mục thất bại" });
        }

        // PUT: api/Category/Edit
        [HttpPut("Edit")]
        public async Task<IActionResult> Edit([FromBody] Category model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });

            var result = await _categoryService.UpdateCategoryAsync(model);
            if (result)
                return Ok(new { message = "Cập nhật danh mục thành công" });

            return StatusCode(500, new { message = "Cập nhật danh mục thất bại" });
        }

        // DELETE: api/Category/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeactivateCategoryAsync(id);
            if (result)
                return Ok(new { message = "Xóa danh mục thành công" });

            return StatusCode(500, new { message = "Xóa danh mục thất bại" });
        }
    }
}
