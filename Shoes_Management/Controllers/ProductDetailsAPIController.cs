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
            // Lấy thông tin sản phẩm và chi tiết sản phẩm với các liên kết cần thiết
            var product = await _context.Products
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
                SizeName = pd.Size?.SizeName,   // Tên kích thước
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

        [HttpGet("GetProductReviews/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            // Lấy chi tiết sản phẩm kèm theo các đánh giá của nó
            var productDetail = await _context.ProductDetails
                .Include(pd => pd.Reviews)
                    .ThenInclude(r => r.Customer)  // Liên kết với khách hàng
                .FirstOrDefaultAsync(pd => pd.ProductId == productId);

            if (productDetail == null)
            {
                return NotFound(new { success = false, message = "Chi tiết sản phẩm không tồn tại." });
            }

            // Lấy các đánh giá và tính điểm trung bình
            var reviews = productDetail.Reviews.ToList();
            double averageRating = reviews.Any() ? reviews.Average(r => r.Rating ?? 0) : 5; // Mặc định 5 sao nếu không có đánh giá

            var response = new
            {
                success = true,
                averageRating = Math.Round(averageRating, 1),  // Làm tròn điểm trung bình
                totalReviews = reviews.Count,
                reviews = reviews.Select(r => new
                {
                    r.Customer.CustomerId,
                    r.Customer.Name,  // Thông tin tên khách hàng
                    r.Rating,
                    r.Comment,
                    DatePosted = r.ReviewDate?.ToString("dd/MM/yyyy") ?? "N/A",
                    r.Status
                })
            };

            return Ok(response);
        }

        [HttpGet("CheckLoginStatus")]
        public IActionResult CheckLoginStatus()
        {
            var accountId = HttpContext.Session.GetString("acc_id");

            return Ok(new { isLoggedIn = accountId != null });
        }

        [HttpPost("ToggleWishlist")]
        public async Task<IActionResult> ToggleWishlist([FromBody] int productId)
        {
            var accountId = HttpContext.Session.GetString("acc_id");
            if (accountId == null)
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId.ToString() == accountId);
            if (customer == null)
            {
                return NotFound(new { message = "Khách hàng không tồn tại." });
            }

            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.CustomerId == customer.CustomerId && w.ProductId == productId);

            if (wishlistItem != null)
            {
                _context.Wishlists.Remove(wishlistItem);
            }
            else
            {
                _context.Wishlists.Add(new Wishlist
                {
                    ProductId = productId,
                    CustomerId = customer.CustomerId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = wishlistItem != null ? "Sản phẩm đã được xóa khỏi danh sách yêu thích." : "Sản phẩm đã được thêm vào danh sách yêu thích.", isFavorite = wishlistItem == null });
        }

        [HttpGet("CheckWishlistStatus/{productId}")]
        public async Task<IActionResult> CheckWishlistStatus(int productId)
        {
            var accountId = HttpContext.Session.GetString("acc_id");
            if (accountId == null)
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để kiểm tra trạng thái yêu thích." });
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountId.ToString() == accountId);

            if (customer == null)
            {
                return NotFound(new { message = "Khách hàng không tồn tại." });
            }

            var isFavorite = await _context.Wishlists
                .AnyAsync(w => w.CustomerId == customer.CustomerId && w.ProductId == productId);

            return Ok(new { isFavorite });
        }
    }
}
