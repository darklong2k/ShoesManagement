using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shoes_Management.Models;

namespace Shoes_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerAPIController : ControllerBase
    {
        private readonly Shoescontext _context;
        public CustomerAPIController(Shoescontext context)
        {
            _context = context;
        }

        [HttpGet("PersonalPage")]
        public IActionResult PersonalPage()
        {
            // Lấy thông tin Khách hàng đã đăng nhập
            var acc_id = HttpContext.Session.GetString("acc_id");
            var custumerInFo = _context.Customers.FirstOrDefault(c => c.AccountId.ToString() == acc_id);
            var orderInFo = (from od in _context.Orders
                             join ct in _context.Customers on od.CustomerId equals ct.CustomerId
                             join ac in _context.Accounts on ct.AccountId equals ac.AccountId
                             where ac.AccountId.ToString() == acc_id
                             select new
                             {
                                 OrderId = od.OrderId,
                                 OrderDate = od.OrderDate,
                                 Status = od.Status

                             }).ToList();
            var likedProducts = (from ct in _context.Customers
                                 join wl in _context.Wishlists on ct.CustomerId equals wl.CustomerId
                                 join pd in _context.Products on wl.ProductId equals pd.ProductId
                                 join acc in _context.Accounts on ct.AccountId equals acc.AccountId
                                 where acc.AccountId.ToString() == acc_id
                                 select new
                                 {
                                     CustomerId = ct.CustomerId,
                                     ProductName = pd.Name,
                                     ProdcutPrice = pd.Price,
                                     ProductImage = pd.Image,
                                     ProdcuctId = pd.ProductId
                                 }).ToList();

            return Ok(new { success = true, custumerInFo, orderInFo, likedProducts });
        }


        [HttpPost("{orderId}")]
        public IActionResult CancelOrder(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound(new { success = false, message = "Đơn hàng không tồn tại." });
            }

            if (order.Status == "Pending" || order.Status == "Packing")
            {
                order.Status = "Cancelled";
                _context.SaveChanges();
                return Ok(new { success = true, message = "Đơn hàng đã được hủy." });
            }

            return BadRequest(new { success = false, message = "Không thể hủy đơn hàng ở trạng thái này." });
        }

        [HttpPut("updatePersonalInfo/{id}")]
        public IActionResult UpdatePersonalInfo(int id, [FromBody] Customer updatedInfo)
        {
            // Kiểm tra xem thông tin khách hàng có hợp lệ không
            if (updatedInfo == null || updatedInfo.CustomerId <= 0)
            {
                return BadRequest(new { success = false, message = "Thông tin không hợp lệ." });
            }

            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == updatedInfo.CustomerId);
            if (customer == null)
            {
                return NotFound(new { success = false, message = "Khách hàng không tìm thấy." });
            }

            // Kiểm tra xem email có bị trùng với email của khách hàng khác không
            var existingEmail = _context.Customers
                .FirstOrDefault(c => c.Email == updatedInfo.Email && c.CustomerId != updatedInfo.CustomerId);
            if (existingEmail != null)
            {
                return BadRequest(new { success = false, message = "Email này đã được sử dụng bởi khách hàng khác." });
            }

            // Kiểm tra xem số điện thoại có bị trùng với số điện thoại của khách hàng khác không
            var existingPhone = _context.Customers
                .FirstOrDefault(c => c.Phone == updatedInfo.Phone && c.CustomerId != updatedInfo.CustomerId);
            if (existingPhone != null)
            {
                return BadRequest(new { success = false, message = "Số điện thoại này đã được sử dụng bởi khách hàng khác." });
            }

            // Cập nhật các thông tin
            customer.Name = updatedInfo.Name;
            customer.Email = updatedInfo.Email;
            customer.Phone = updatedInfo.Phone;
            customer.Address = updatedInfo.Address;
            customer.Sex = updatedInfo.Sex;

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            return Ok(new { success = true, message = "Thông tin cá nhân đã được cập nhật." });
        }
        [HttpDelete("RemoveFromWishlist/{productId}/{customerId}")]
        public IActionResult RemoveFromWishlist(int productId, int customerId)
        {
            // Tìm sản phẩm trong danh sách yêu thích của khách hàng
            var wishlistItem = _context.Wishlists
                .FirstOrDefault(w => w.ProductId == productId && w.CustomerId == customerId);

            if (wishlistItem == null)
            {
                return NotFound(new { success = false, message = "Sản phẩm không tìm thấy trong danh sách yêu thích." });
            }

            // Xóa sản phẩm khỏi danh sách yêu thích
            _context.Wishlists.Remove(wishlistItem);
            _context.SaveChanges();

            return Ok(new { success = true, message = "Sản phẩm đã được bỏ yêu thích." });
        }

    }
}
