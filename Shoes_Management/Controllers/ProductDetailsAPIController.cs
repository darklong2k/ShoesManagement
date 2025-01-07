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

        [HttpGet("GetProductDetails/{productId}")]
        public async Task<IActionResult> GetProductDetails(int productId)
        {
            // Lấy thông tin sản phẩm và chi tiết sản phẩm
            var product = await _context.Products
                .Include(p => p.ProductDetails)
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.Status == "Active");

            if (product == null)
            {
                return Ok(new { success = false, message = "Sản phẩm không tồn tại hoặc đã bị ẩn" });
            }

            // Định dạng phản hồi
            var productDetails = product.ProductDetails.Select(pd => new
            {
                pd.Size,
                pd.Color,
                pd.StockQuantity
            });

            var response = new
            {
                success = true,
                product = new
                {
                    product.Name,
                    product.Description,
                    product.Price,
                    product.Brand,
                    product.Category,
                    product.Status,
                    ProductDetails = productDetails
                }
            };

            return Ok(response);
        }
    }
}
