using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Models;

namespace Shoes_Management.Areas.Admin.Controllers
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
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _context.Categories
                    .ToListAsync();

                return Ok(categories); // Trả về kết quả dưới dạng JSON
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

        // POST: api/Category/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] Category model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Invalid model data." });

            _context.Categories.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Category created successfully." });
        }

        // PUT: api/Category/Edit
        [HttpPut("Edit")]
        public async Task<IActionResult> Edit([FromBody] Category model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Invalid model data." });

            var category = await _context.Categories.FindAsync(model.CategoryId);
            if (category == null)
                return NotFound(new { Success = false, Message = "Category not found." });

            category.Name = model.Name;
            category.ParentId = model.ParentId;
            category.Description = model.Description;
            category.Status = model.Status;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Category updated successfully." });
        }

        // DELETE: api/Category/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { Success = false, Message = "Category not found." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Category deleted successfully." });

        }
    }
}
