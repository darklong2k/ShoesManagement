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
                .Include(p => p.ProductDetails)
                              .ThenInclude(pd => pd.Size)
                          .Include(p => p.ProductDetails)
                              .ThenInclude(pd => pd.Color)
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.Status == "Active");

            if (product == null)
            {
                return Ok(new { success = false, message = "Sản phẩm không tồn tại hoặc đã bị ẩn" });
            }

            // Định dạng phản hồi
            var productDetails = product.ProductDetails.Select(pd => new
            {
                SizeId = pd.Size?.SizeId,       // Mã kích thước
                SizeName = pd.Size?.SizeName,       // Tên kích thước
                ColorId = pd.Color?.ColorId,   // Mã màu
                ColorName = pd.Color?.ColorName, // Tên màu
                StockQuantity = pd.StockQuantity // Số lượng tồn kho
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
        // GET: api/reviews/{productId}
        [HttpGet("GetProductReviews/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            // Lấy chi tiết sản phẩm kèm theo các đánh giá của nó
            var productDetail = await _context.ProductDetails
                .Include(pd => pd.Reviews)
                .ThenInclude(r => r.Customer)  // Liên kết với khách hàng để lấy thông tin tên khách hàng
                .FirstOrDefaultAsync(pd => pd.ProductId == productId);

            if (productDetail == null)
            {
                return NotFound(new { success = false, message = "Chi tiết sản phẩm không tồn tại." });
            }

            // Lấy các đánh giá
            var reviews = productDetail.Reviews.ToList();
            double averageRating = reviews.Any() ? reviews.Average(r => r.Rating ?? 0) : 5; // Nếu không có đánh giá, điểm trung bình là 5

            // Kiểm tra nếu không có đánh giá nào
            if (reviews.Count == 0)
            {
                return Ok(new
                {
                    success = true,
                    message = "Không có lượt đánh giá nào",
                    averageRating = 5, // Mặc định là 5 sao
                    totalReviews = 0,
                    reviews = new List<object>()
                });
            }

            var response = new
            {
                success = true,
                averageRating = Math.Round(averageRating, 1),  // Làm tròn điểm trung bình
                totalReviews = reviews.Count,
                reviews = reviews.Select(r => new
                {
                    r.Customer.CustomerId,
                    r.Customer.Name,  // Thêm thông tin tên khách hàng
                    r.Rating,
                    r.Comment,
                    DatePosted = r.ReviewDate?.ToString("dd/MM/yyyy") ?? "N/A",
                    r.Status
                })
            };

            return Ok(response);
        }
    }
}
