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
                             //join ac in _context.Accounts on ct.AccountId equals ac.AccountId
                             where ct.AccountId.ToString() == acc_id
                             select new
                             {
                                 OrderId = od.OrderId,
                                 OrderDate = od.OrderDate,
                                 Status = od.Status
                             }).ToList();
            var likedProducts = (from ct in _context.Customers
                                 join wl in _context.Wishlists on ct.CustomerId equals wl.CustomerId
                                 join pd in _context.Products on wl.ProductId equals pd.ProductId
                                 //join acc in _context.Accounts on ct.AccountId equals acc.AccountId
                                 where ct.AccountId.ToString() == acc_id
                                 select new
                                 {
                                     CustomerId = ct.CustomerId,
                                     ProductName = pd.Name,
                                     ProdcutPrice = pd.Price,
                                     ProductImage = pd.Image,
                                     ProdcuctId = pd.ProductId
                                 }).ToList();
            var ratedProducts = (from rv in _context.Reviews
                                 join ct in _context.Customers on rv.CustomerId equals ct.CustomerId
                                 join pdd in _context.ProductDetails on rv.ProductDetailId equals pdd.ProductDetailId
                                 join pd in _context.Products on pdd.ProductId equals pd.ProductId
                                 where ct.AccountId.ToString() == acc_id
                                 select new
                                 {
                                     ProductName = pd.Name,
                                     ProductPrice = pd.Price,
                                     ProductImage = pd.Image,
                                     Rating = rv.Rating
                                 }).ToList();


            // Trả về dữ liệu
            return Ok(new
            {
                success = true,
                custumerInFo,
                orderInFo,
                likedProducts,
                ratedProducts
            });
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
        [HttpGet("OrderDetails/{orderId}")]
        public IActionResult GetOrder(int orderId)
        {
            try
            {
                // Truy vấn dữ liệu từ cơ sở dữ liệu
                var orderDetails = (from od in _context.Orders
                                    join oddt in _context.OrderDetails on od.OrderId equals oddt.OrderId
                                    join pddt in _context.ProductDetails on oddt.ProductDetailId equals pddt.ProductDetailId
                                    join pd in _context.Products on pddt.ProductId equals pd.ProductId
                                    join rv in _context.Reviews on pddt.ProductDetailId equals rv.ProductDetailId into reviews
                                    join co in _context.Colors on pddt.ColorId equals co.ColorId
                                    join s in _context.Sizes on pddt.SizeId equals s.SizeId
                                    from rv in reviews.DefaultIfEmpty() // Sử dụng Left Join
                                    where od.OrderId == orderId
                                    select new
                                    {
                                        OrderId = od.OrderId,
                                        ProductDetailId = oddt.ProductDetailId,
                                        ProductName = pd.Name,
                                        ProductDescription = pd.Description,
                                        Price = pd.Price,
                                        SizeName = s.SizeName,
                                        ColorName = co.ColorName,
                                        Comment = rv != null ? rv.Comment : null
                                    }).ToList();

                // Kiểm tra kết quả
                if (!orderDetails.Any())
                {
                    return NotFound(new { success = false, message = "Không tìm thấy chi tiết đơn hàng." });
                }

                // Trả về kết quả
                return Ok(new { success = true, orderDetails });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi trong quá trình xử lý.", error = ex.Message });
            }
        }
        [HttpPut("updatePersonalInfo/{id}")]
        public IActionResult UpdatePersonalInfo(int id, [FromBody] Customer updatedInfo)
        {
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
