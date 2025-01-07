using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shoes_Management.Models;

namespace Shoes_Management.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoardAPIController : ControllerBase
    {
        private readonly Shoescontext _context;
        public DashBoardAPIController(Shoescontext context)
        {
            _context = context;
        }
        [HttpGet("TotalQuantity")]
        public IActionResult GetTotalQuantity() {
            // Tính tổng số lượng từ OrderDetails
            var totalQuantitySold = _context.OrderDetails
                                            .Where(od => od.Quantity.HasValue) // Bỏ qua các giá trị null
                                            .Sum(od => od.Quantity.Value); // Tính tổng

            return Ok(new
            {
                success = true,
                totalQuantitySold
            });

        }
        [HttpGet("TotalRevenue")]
        public IActionResult GetTotalRevenue()
        {
            // Tính tổng doanh thu từ cột TotalPrice
            var totalRevenue = _context.OrderDetails
                                       .Where(od => od.TotalPrice.HasValue) // Bỏ qua giá trị null
                                       .Sum(od => od.TotalPrice.Value);    // Tính tổng

            return Ok(new
            {
                success = true,
                totalRevenue
            });
        }
        [HttpGet("TotalProducts")]
        public IActionResult GetTotalProducts()
        {
            // Đếm tổng số sản phẩm trong bảng ProductDetails
            var totalProducts = _context.ProductDetails.Count();

            return Ok(new
            {
                success = true,
                totalProducts
            });
        }

    }
}
