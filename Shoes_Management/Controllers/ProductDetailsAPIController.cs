using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Models;

namespace Shoes_Management.Controllers
{
    [Route("api/user/[controller]")]
    [ApiController]
    public class ProductDetailsAPIController : ControllerBase
    {
        private readonly Shoescontext _context;

        public ProductDetailsAPIController(Shoescontext context)
        {
            _context = context;
        }

        [HttpGet("productId")]
        public IActionResult GetProductDetails(int productId)
        {
            var product = _context.Products
            .Include(p => p.ProductDetails)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound();
            }

            
            return Ok(new { productId });
        }
    }
}
