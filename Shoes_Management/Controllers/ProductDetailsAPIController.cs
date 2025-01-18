using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Models;
using static Shoes_Management.Controllers.ProductDetailsAPIController;

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
                return Ok(new { success = false, message = "Sản phẩm không tồn tại " });
            }

            // Định dạng phản hồi
            var productDetails = product.ProductDetails.Select(pd => new
            {
                productDetailId = pd.ProductDetailId,
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
                    product.Image,
                    product.Name,
                    product.Description,
                    product.Price,
                    product.Brand,
                    product.Category,
                    product.Status,
                    ProductDetails = productDetails
                },
                likes = GetFavoriteCount(productId)
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

        private int GetFavoriteCount(int productId)
        {
            return _context.Wishlists.Count(w => w.ProductId == productId);
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

            return Ok(new { message = wishlistItem != null ? "Sản phẩm đã được xóa khỏi danh sách yêu thích." : "Sản phẩm đã được thêm vào danh sách yêu thích.", isFavorite = wishlistItem == null, newFavoriteCount = GetFavoriteCount(productId) });
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

            return Ok(new { isFavorite, favoriteCount = GetFavoriteCount(productId) });
        }



        [HttpGet("GetRelatedProducts/{productId}")]
        public IActionResult GetRelatedProducts(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid product ID." });
            }

            // Lấy sản phẩm hiện tại
            var currentProduct = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (currentProduct == null)
            {
                return Ok(new { success = false, message = "Product not found." });
            }

            // Lấy danh sách sản phẩm liên quan
            var relatedProducts = _context.Products
                .Where(p => p.CategoryId == currentProduct.CategoryId && p.ProductId != productId)
                .OrderBy(_ => Guid.NewGuid())
                .Take(4) // Giới hạn số lượng sản phẩm
                .ToList();

            if (!relatedProducts.Any())
            {
                return Ok(new { success = true, relatedProducts = new List<object>() });
            }

            // Chuẩn bị kết quả trả về
            var result = relatedProducts.Select(p => new
            {
                p.ProductId,
                p.Name,
                p.Image,
                p.Price
            });

            return Ok(new { success = true, relatedProducts = result });
        }

        [HttpPost("IncreaseProductView/{productId}")]
        public IActionResult IncreaseProductView(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid product ID." });
            }

            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound(new { success = false, message = "Product not found." });
            }

            product.Views += 1; // Tăng số lượt xem
            _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

            return Ok(new { success = true, message = "Product view increased successfully.", views = product.Views });
        }

        // API Thêm sản phẩm vào giỏ hàng
        [HttpPost("addProductToCart")]
        public async Task<IActionResult> AddProductToCart([FromBody] AddProductToCartRequest request)
        {
            // Lấy accountId từ session
            var accountId = HttpContext.Session.GetString("acc_id");
            if (accountId == null)
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            // Tìm thông tin khách hàng từ AccountId
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountId.ToString() == accountId);
            if (customer == null)
            {
                return NotFound(new { message = "Khách hàng không tồn tại." });
            }

            if (request == null || request.ProductId == 0 || request.ProductDetailId == 0 || request.Quantity <= 0)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                // Kiểm tra sản phẩm và chi tiết sản phẩm có tồn tại không
                var productDetail = await _context.ProductDetails
                    .Include(pd => pd.Product) // Include Product để tránh truy vấn thêm lần nữa
                    .FirstOrDefaultAsync(pd => pd.ProductDetailId == request.ProductDetailId);

                if (productDetail == null || productDetail.Product == null)
                {
                    return BadRequest(new { success = false, message = "Sản phẩm hoặc chi tiết sản phẩm không hợp lệ." });
                }

                var price = productDetail.Product.Price;
                var stockQuantity = productDetail.StockQuantity;

                // Kiểm tra số lượng yêu cầu có vượt quá tồn kho
                if (request.Quantity > stockQuantity)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Bạn chỉ có thể thêm tối đa {stockQuantity} sản phẩm vào giỏ hàng."
                    });
                }

                // Kiểm tra xem sản phẩm đã có trong giỏ hàng của khách hàng chưa
                var existingCartDetail = await _context.CartDetails
                    .FirstOrDefaultAsync(cd => cd.CustomerId == customer.CustomerId && cd.ProductDetailId == request.ProductDetailId);

                if (existingCartDetail != null)
                {
                    // Nếu sản phẩm đã có trong giỏ hàng, kiểm tra số lượng có hợp lệ không
                    if (existingCartDetail.Quantity + request.Quantity > stockQuantity)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"Bạn chỉ có thể thêm tối đa {stockQuantity - existingCartDetail.Quantity} sản phẩm vào giỏ hàng."
                        });
                    }

                    // Cập nhật số lượng sản phẩm trong giỏ hàng
                    existingCartDetail.Quantity += request.Quantity;
                    existingCartDetail.TotalPrice = existingCartDetail.Quantity * price;
                    existingCartDetail.CreatedAt = DateTime.UtcNow;

                    _context.CartDetails.Update(existingCartDetail);
                }
                else
                {
                    // Nếu sản phẩm chưa có trong giỏ hàng, thêm mới
                    var newCartDetail = new CartDetail
                    {
                        CustomerId = customer.CustomerId,
                        ProductDetailId = request.ProductDetailId,
                        Quantity = request.Quantity,
                        TotalPrice = request.Quantity * price,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.CartDetails.Add(newCartDetail);
                }

                // Lưu thay đổi vào CSDL
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Sản phẩm đã được thêm vào giỏ hàng." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // Dữ liệu gửi từ phía client
        public class AddProductToCartRequest
        {
            public int ProductId { get; set; }
            public int ProductDetailId { get; set; }
            public int Quantity { get; set; }
        }



    }
}




