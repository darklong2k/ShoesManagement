using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shoes_Management.Models;

namespace Shoes_Management.Areas.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private Shoescontext _shoescontext; 
        public BrandController(Shoescontext shoescontext)
        {
            _shoescontext = shoescontext;
        }
        [HttpPost("Create")]
        public IActionResult Create([FromBody] Brand brand)
        {
            if (brand == null || string.IsNullOrEmpty(brand.BrandName))
            {
                return BadRequest("Invalid brand data.");
            }

            // Kiểm tra trùng tên
            var existingBrand = _shoescontext.Brands.FirstOrDefault(b => b.BrandName == brand.BrandName);
            if (existingBrand != null)
            {
                return Conflict(new { Success = false, Message = "Tên thương hiệu đã tồn tại." });
            }

            _shoescontext.Brands.Add(brand);
            _shoescontext.SaveChanges();
            return Ok(new { Success = true, Data = brand });
        }
        [HttpGet("Getall")]
        public IActionResult Getall()
        {
            return Ok(new { data = _shoescontext.Brands.ToList() });
        }
    }
}
