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

        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            // Kiểm tra người dùng đã đăng nhập chưa
            var accountId = HttpContext.Session.GetString("acc_id");
            if (accountId == null)
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            // Tìm khách hàng từ AccountId
            var customer = await _context.Customers
                                          .FirstOrDefaultAsync(c => c.AccountId.ToString() == accountId);
            if (customer == null)
            {
                return NotFound(new { message = "Khách hàng không tồn tại." });
            }

            // Kiểm tra xem sản phẩm có tồn tại không
            var productDetail = await _context.ProductDetails
                 .Include(pd => pd.Product)
                     .ThenInclude(r => r.Price)
                 .FirstOrDefaultAsync(pd => pd.ProductDetailId == request.ProductDetailId);

            if (productDetail == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại." });
            }

            // Kiểm tra nếu sản phẩm không có giá trị hợp lệ
            if (productDetail.Product.Price <= 0)
            {
                return BadRequest(new { message = "Giá sản phẩm không hợp lệ." });
            }

            // Kiểm tra xem giỏ hàng của người dùng đã có sản phẩm này chưa
            var existingCartItem = await _context.CartDetails
                                                  .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId && c.ProductDetailId == request.ProductDetailId);

            if (existingCartItem != null)
            {
                // Nếu có sản phẩm trong giỏ, cộng thêm số lượng
                existingCartItem.Quantity += request.Quantity;
                existingCartItem.TotalPrice = existingCartItem.Quantity * productDetail.Product.Price;
                existingCartItem.CreatedAt = DateTime.Now;
                _context.CartDetails.Update(existingCartItem);
            }
            else
            {
                // Nếu không có sản phẩm trong giỏ, thêm mới
                var newCartItem = new CartDetail
                {
                    CustomerId = customer.CustomerId,
                    ProductDetailId = request.ProductDetailId,
                    Quantity = request.Quantity,
                    TotalPrice = request.Quantity * productDetail.Product.Price,
                    CreatedAt = DateTime.Now
                };
                await _context.CartDetails.AddAsync(newCartItem);
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sản phẩm đã được thêm vào giỏ hàng." });
        }
    }
}
