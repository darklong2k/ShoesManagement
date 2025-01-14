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

        // Helper function to get customer by session account ID
        private async Task<Customer> GetCustomerFromSession()
        {
            var accountId = HttpContext.Session.GetString("acc_id");
            if (accountId == null)
            {
                return null;
            }

            return await _context.Customers.FirstOrDefaultAsync(c => c.AccountId.ToString() == accountId);
        }

        [HttpGet("getCartItems")]
        public async Task<IActionResult> GetCartItems()
        {
            var customer = await GetCustomerFromSession();
            if (customer == null)
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            var cartItems = await _context.CartDetails
                .Where(cd => cd.CustomerId == customer.CustomerId)
                .Include(cd => cd.ProductDetail)
                .ThenInclude(pd => pd.Product)
                .Include(cd => cd.ProductDetail)
                .ThenInclude(pd => pd.Color)
                .Include(cd => cd.ProductDetail)
                .ThenInclude(pd => pd.Size)
                .Select(cd => new
                {
                    cd.ProductDetailId,
                    cd.Quantity,
                    ProductName = cd.ProductDetail.Product.Name,
                    ProductImage = cd.ProductDetail.Product.Image,
                    ProductColor = cd.ProductDetail.Color.ColorName,
                    ProductSize = cd.ProductDetail.Size.SizeName,
                    ProductPrice = cd.ProductDetail.Product.Price
                })
                .ToListAsync();

            return Ok(new { success = true, cartItems });
        }

        [HttpDelete("removeItem/{productDetailId}")]
        public async Task<IActionResult> RemoveItem(int productDetailId)
        {
            var customer = await GetCustomerFromSession();
            if (customer == null)
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            var item = await _context.CartDetails
                .FirstOrDefaultAsync(cd => cd.ProductDetailId == productDetailId && cd.CustomerId == customer.CustomerId);

            if (item == null)
            {
                return NotFound(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng của bạn." });
            }

            _context.CartDetails.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Sản phẩm đã được xóa khỏi giỏ hàng." });
        }

        [HttpDelete("clearCart")]
        public async Task<IActionResult> ClearCart()
        {
            var customer = await GetCustomerFromSession();
            if (customer == null)
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            var cartItems = await _context.CartDetails
                .Where(cd => cd.CustomerId == customer.CustomerId)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                return Ok(new { success = false, message = "Giỏ hàng đã trống." });
            }

            _context.CartDetails.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Giỏ hàng đã được xóa." });
        }

        public class UpdateQuantityRequest
        {
            public int Quantity { get; set; }
        }

        [HttpPut("updateQuantity/{productDetailId}")]
        public async Task<IActionResult> UpdateQuantity(int productDetailId,[FromBody] UpdateQuantityRequest request)
        {
            var customer = await GetCustomerFromSession();
            if (customer == null)
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            var item = await _context.CartDetails
                .FirstOrDefaultAsync(cd => cd.ProductDetailId == productDetailId && cd.CustomerId == customer.CustomerId);

            if (item == null)
            {
                return NotFound(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng của bạn." });
            }

            item.Quantity = Math.Max(1, request.Quantity); // Ensure quantity is at least 1
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpGet("getTotalPrice")]
        public async Task<IActionResult> GetTotalPrice()
        {
            var customer = await GetCustomerFromSession();
            if (customer == null)
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            var cartItems = await _context.CartDetails
                .Where(cd => cd.CustomerId == customer.CustomerId)
                .Include(cd => cd.ProductDetail)
                .ThenInclude(pd => pd.Product)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                return Ok(new { success = false, message = "Giỏ hàng trống." });
            }

            decimal totalPrice = (decimal)cartItems.Sum(item => item.ProductDetail.Product.Price * item.Quantity);


            return Ok(new { success = true, totalPrice });
        }
    }
}
