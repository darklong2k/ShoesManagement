using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Shoes_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        private readonly Shoescontext _context;

        public OrderAPIController(Shoescontext context)
        {
            _context = context;
        }



        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest info)
        {
            try
            {
                // Kiểm tra session và lấy accountId
                var accountId = HttpContext.Session.GetString("acc_id");
                if (accountId == null)
                {
                    return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện thao tác này." });
                }

                // Lấy thông tin khách hàng
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId.ToString() == accountId);
                if (customer == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin khách hàng." });
                }

                // Lấy chi tiết giỏ hàng của khách hàng
                var cartDetails = _context.CartDetails
                    .Where(cd => cd.CustomerId == customer.CustomerId)
                    .Select(cd => new
                    {
                        ProductName = cd.ProductDetail.Product.Name,
                        cd.Quantity,
                        UnitPrice = cd.ProductDetail.Product.Price,
                        TotalPrice = cd.TotalPrice
                    })
                    .ToList();

                // Kiểm tra xem giỏ hàng có sản phẩm không
                if (!cartDetails.Any())
                {
                    return BadRequest(new { message = "Giỏ hàng của bạn không có sản phẩm." });
                }

                // Tính tổng tiền giỏ hàng
                var totalPrice = cartDetails.Sum(cd => cd.TotalPrice ?? 0);

                // Tạo đơn hàng mới
                var order = new Order
                {
                    CustomerId = customer.CustomerId,
                    OrderDate = DateTime.Now,
                    Address = info.Address,
                    Phone = info.Phone,
                    Note = info.Note,
                    TotalAmount = totalPrice, // Tổng tiền
                    Status = "Pending" // Trạng thái chờ xử lý
                };

                // Lưu đơn hàng vào cơ sở dữ liệu
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Tạo thanh toán mới
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethodId = info.PaymentMethod, // ID phương thức thanh toán
                    PaymentDate = DateTime.Now,
                    TotalAmount = totalPrice, // Tổng tiền thanh toán
                    Status = "Chưa thanh toán" // Trạng thái chưa thanh toán
                };

                // Lưu thanh toán vào cơ sở dữ liệu
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Lấy chi tiết giỏ hàng và xử lý từng sản phẩm trong giỏ hàng
                var cartItems = await GetCartItems(customer.CustomerId); // Dùng await để đảm bảo async được xử lý đúng

                decimal totalAmount = 0;
                foreach (var item in cartItems)
                {
                    // Tạo chi tiết đơn hàng
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductDetailId = item.ProductDetailId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    };

                    // Lưu chi tiết đơn hàng vào cơ sở dữ liệu
                    _context.OrderDetails.Add(orderDetail);
                    totalAmount += item.TotalPrice ?? 0;

                    // Cập nhật tồn kho sản phẩm
                    var productDetail = await _context.ProductDetails
                        .FirstOrDefaultAsync(pd => pd.ProductDetailId == item.ProductDetailId);

                    if (productDetail != null)
                    {
                        productDetail.StockQuantity -= item.Quantity;
                        _context.ProductDetails.Update(productDetail);
                    }
                }

                // Cập nhật tổng tiền cho đơn hàng và thanh toán
                order.TotalAmount = totalAmount;
                payment.TotalAmount = totalAmount;

                // Lưu lại các thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                // Xóa giỏ hàng của khách hàng sau khi đặt hàng thành công
                await ClearCart(customer.CustomerId); // Dùng await để gọi hàm async

                // Trả về kết quả thành công
                return Ok(new { message = "Đặt hàng thành công!", orderId = order.OrderId });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                return BadRequest(new { message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // Hàm lấy giỏ hàng của khách hàng
        private async Task<List<CartItem>> GetCartItems(int customerId)
        {
            var cartItems = await _context.CartDetails
                .Where(cd => cd.CustomerId == customerId)
                .Select(cd => new CartItem
                {
                    ProductDetailId = cd.ProductDetailId,
                    Quantity = cd.Quantity,
                    UnitPrice = cd.ProductDetail.Product.Price,
                    TotalPrice = cd.TotalPrice
                })
                .ToListAsync();

            return cartItems;
        }

        // Hàm xóa giỏ hàng của khách hàng
        private async Task ClearCart(int customerId)
        {
            var cartItems = await _context.CartDetails.Where(ci => ci.CustomerId == customerId).ToListAsync();
            _context.CartDetails.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        // Hàm trang thành công sau khi thanh toán
        [HttpGet("PaymentSuccess")]
        public IActionResult PaymentSuccess(int orderId)
        {
            return Ok(new { message = "Thanh toán thành công!", orderId = orderId });
        }
    }

    // Lớp đại diện cho chi tiết giỏ hàng
    public class CartItem
    {
        public int ProductDetailId { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
    }
    public class OrderRequest
    {
        public int PaymentMethod { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
    }
}
