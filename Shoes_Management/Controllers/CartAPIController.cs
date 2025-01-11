using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Models;

namespace Shoes_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private readonly Shoescontext _context;

        public CartAPIController(Shoescontext context)
        {
            _context = context;
        }

        public class AddToCartRequest
        {
            public int ProductDetailId { get; set; }
            public int Quantity { get; set; }
        }

        [HttpGet("GetCartDetails")]
        public async Task<IActionResult> GetCartDetails()
        {
            var accountId = HttpContext.Session.GetString("acc_id");
            if (accountId == null)
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để sử dụng giỏ hàng." });
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountId.ToString() == accountId);

            if (customer == null)
            {
                return NotFound(new { message = "Khách hàng không tồn tại." });
            }

            var cartDetails = await _context.CartDetails
                .Include(cd => cd.ProductDetail) // Load thông tin chi tiết sản phẩm
                .ThenInclude(pd => pd.Product)  // Load thông tin sản phẩm
                .Where(cd => cd.CustomerId == customer.CustomerId)
                .ToListAsync();

            if (!cartDetails.Any())
            {
                return NotFound(new { message = "Giỏ hàng rỗng hoặc không tồn tại." });
            }

            var result = cartDetails.Select(cd => new
            {
                cd.CustomerId,
                cd.ProductDetailId,
                cd.Quantity,
                cd.TotalPrice,
                cd.CreatedAt,
                Product = new
                {
                    ProductName = cd.ProductDetail.Product.Name,
                    Color = cd.ProductDetail.Color,
                    Size = cd.ProductDetail.Size,
                    Price = cd.ProductDetail.Product.Price,
                    Image = cd.ProductDetail.Product.Image
                }
            });

            return Ok(result);
        }
        [HttpGet("GetCartSummary")]
        public async Task<IActionResult> GetCartSummary()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            var accountId = HttpContext.Session.GetString("acc_id");
            if (accountId == null)
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để xem giỏ hàng." });
            }

            // Lấy khách hàng dựa trên tài khoản
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountId.ToString() == accountId);

            if (customer == null)
            {
                return NotFound(new { message = "Khách hàng không tồn tại." });
            }

            // Lấy thông tin giỏ hàng của khách hàng
            var cartDetails = await _context.CartDetails
                .Include(cd => cd.ProductDetail) // Lấy thông tin chi tiết sản phẩm
                .ThenInclude(pd => pd.Product)  // Lấy thông tin sản phẩm
                .Where(cd => cd.CustomerId == customer.CustomerId)
                .ToListAsync();

            if (cartDetails == null || !cartDetails.Any())
            {
                return Ok(new { totalItems = 0, totalPrice = 0 });
            }

            // Tính tổng số lượng sản phẩm và tổng tiền
            int totalItems = cartDetails.Select(cd => cd.ProductDetail.ProductId).Distinct().Count();
            decimal totalPrice = cartDetails.Sum(cd => (cd.Quantity ?? 0) * (cd.TotalPrice ?? 0));

            return Ok(new
            {
                totalItems = totalItems,
                totalPrice = totalPrice
            });
        }
    }
    }

