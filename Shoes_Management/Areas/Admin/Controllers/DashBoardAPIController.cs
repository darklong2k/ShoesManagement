using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Models;
using System.Text.RegularExpressions;

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
        [HttpGet("TotalOrders/{year}")]
        public IActionResult TotalOrders(int year) {
            // Tính tổng số lượng từ Orders
            var totalQuantitySold = (from ct in _context.Orders
                                     where ct.OrderDate.HasValue && ct.OrderDate.Value.Year == year
                                     && ct.Status == "Delivered"
                                     select ct.OrderId
                                 ).Count();

            return Ok(new
            {
                success = true,
                totalQuantitySold
            });

        }
        [HttpGet("TotalCustomer/{year}")]
        public IActionResult GetTotalCustomer(int year)
        {
            // Tính tổng doanh thu từ cột TotalPrice
            var totalCustomer = (from ct in _context.Customers
                                 select ct.CustomerId
                                 ).Count();

            return Ok(new
            {
                success = true,
                totalCustomer
            });
        }
        [HttpGet("TotalProducts/{year}")]
        public IActionResult GetTotalProducts(int year)
        {
            // Đếm tổng số sản phẩm trong bảng ProductDetails
            var totalProducts = (from ct in _context.Products
                                 select ct.ProductId
                                 ).Count();

            return Ok(new
            {
                success = true,
                totalProducts
            });
        }
        [HttpGet("TotalDeliveredRevenue")]
        public IActionResult GetTotalDeliveredRevenue()
        {
            // Tính tổng doanh thu của các đơn hàng có trạng thái 'Delivered'
            var totalDeliveredRevenue = _context.Orders
                                                 .Join(_context.Payments,
                                                       od => od.OrderId,
                                                       pm => pm.OrderId,
                                                       (od, pm) => new { od, pm })
                                                 .Where(x => x.pm.Status == "Delivered")
                                                 .Sum(x => x.pm.TotalAmount);

            return Ok(new
            {
                success = true,
                totalDeliveredRevenue
            });
        }
        [HttpGet("MonthlyRevenue/{year}")]
        public IActionResult GetMonthlyRevenue(int year)
        {
            decimal[] Month= new decimal[12];
            // Truy vấn để lấy doanh thu theo tháng cho năm cụ thể
            
            for (int i = 1;i<13;i++)
            {
                var revenueData = (from od in _context.Orders
                                   join pm in _context.Payments on od.OrderId equals pm.OrderId
                                   where pm.Status == "Delivered" &&
                                         od.OrderDate.HasValue && od.OrderDate.Value.Year == year
                                         && od.OrderDate.Value.Month == i
                                   select pm.TotalAmount).Sum() ?? 0;
                Month[i-1] = revenueData;
            }

            // Khởi tạo mảng doanh thu cho từng tháng (12 tháng)
            decimal[] monthlyRevenue = new decimal[12];

            

            // Đảm bảo trả về thông tin thành công và doanh thu
            return Ok(new
            {
                success = true,
                revenue = Month
            });
        }
        [HttpGet("MonthlyOrders/{year}")]
        public IActionResult MonthlyOrders(int year)
        {
            decimal[] Month = new decimal[12];
            // Truy vấn để lấy doanh thu theo tháng cho năm cụ thể

            for (int i = 1; i < 13; i++)
            {
                var revenueData = (from od in _context.Orders
                                   where od.OrderDate.HasValue
                                         && od.OrderDate.Value.Year == year
                                         && od.OrderDate.Value.Month == i
                                   select od).Count();

                Month[i - 1] = revenueData;
            }

            // Khởi tạo mảng doanh thu cho từng tháng (12 tháng)
            decimal[] monthlyRevenue = new decimal[12];



            // Đảm bảo trả về thông tin thành công và doanh thu
            return Ok(new
            {
                success = true,
                revenue = Month
            });
        }
        [HttpGet("MonthlyRevenueAndCategory/{year}/{categoryId}")]
        public IActionResult GetMonthlyRevenueAndSex(int year,int categoryId)
        {
            try
            {
                decimal[] Month = new decimal[12];
                // Truy vấn để lấy doanh thu theo tháng cho năm cụ thể

                for (int i = 1; i < 13; i++)
                {
                    // LINQ Query
                    var totalRevenue = (from ct in _context.Categories
                                        join pd in _context.Products on ct.CategoryId equals pd.CategoryId
                                        join pddt in _context.ProductDetails on pd.ProductId equals pddt.ProductId
                                        join oddt in _context.OrderDetails on pddt.ProductDetailId equals oddt.ProductDetailId
                                        join od in _context.Orders on oddt.OrderId equals od.OrderId
                                        join pm in _context.Payments on od.OrderId equals pm.OrderId
                                        where od.OrderDate.HasValue &&
                                              od.OrderDate.Value.Year == year &&
                                              ct.CategoryId == categoryId
                                              && od.OrderDate.Value.Month==i
                                        select pm.TotalAmount).Sum()??0;
                    Month[i - 1] = totalRevenue;
                }

                return Ok(new
                {
                    success = true,
                    revenue = Month
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving revenue", Error = ex.Message });
            }
        }
        [HttpGet("TotalPurchases/{year}/{categoryId}")]
        public IActionResult GetTotalPurchases(int year, int categoryId)
        {
            try
            {
                // Mảng lưu trữ số lượt mua theo tháng
                decimal[] monthPurchases = new decimal[12];

                // Duyệt qua từng tháng để tính lượt mua
                for (int i = 1; i <= 12; i++)
                {
                    // Truy vấn LINQ
                    var totalPurchases = (from ct in _context.Categories
                                          join pd in _context.Products on ct.CategoryId equals pd.CategoryId
                                          join pddt in _context.ProductDetails on pd.ProductId equals pddt.ProductId
                                          join oddt in _context.OrderDetails on pddt.ProductDetailId equals oddt.ProductDetailId
                                          join od in _context.Orders on oddt.OrderId equals od.OrderId
                                          where od.OrderDate.HasValue &&
                                                od.OrderDate.Value.Year == year &&
                                                ct.CategoryId == categoryId &&
                                                od.OrderDate.Value.Month == i
                                          select oddt.ProductDetailId).Count();

                    // Gán kết quả vào mảng
                    monthPurchases[i - 1] = totalPurchases;
                }

                // Trả về kết quả dưới dạng JSON
                return Ok(new
                {
                    success = true,
                    purchases = monthPurchases
                });
            }
            catch (Exception ex)
            {
                // Trả về lỗi nếu có vấn đề xảy ra
                return StatusCode(500, new { Message = "Error retrieving purchase data", Error = ex.Message });
            }
        }


        [HttpGet("GetWebData")]
        public IActionResult GetWebData()
        {
            
            var webData = _context.Websites
                .Where(w => w.WebsiteId == 2)
                .Select(w => new
                {
                    w.WebsiteName,
                    w.WebsiteAddress,
                    w.WebsiteLinkFb,
                    w.WebsiteImage,
                    w.WebsitePhone,
                    w.WebsiteEmail
                })
                .FirstOrDefault();



            // Trả về dữ liệu dưới dạng JSON
            return Ok(new
            {
                success = true,
                webData
            });
        }
        [HttpPut("UpdateWebData")]
        public async Task<IActionResult> UpdateWebsiteInfo( [FromBody] Website websiteInfo)
        {
            if (websiteInfo == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            if (!Regex.IsMatch(websiteInfo.WebsiteName, @"^.{1,255}$"))
            {
                return Ok(new { success = false, message = "Tên website không được vượt quá 255 ký tự." });
            }

            if (!Regex.IsMatch(websiteInfo.WebsiteAddress, @"^.{1,255}$"))
            {
                return Ok(new { success = false, message = "Địa chỉ website không được vượt quá 255 ký tự." });
            }

            if (!Regex.IsMatch(websiteInfo.WebsiteLinkFb, @"^.{1,255}$"))
            {
                return Ok(new { success = false, message = "Link Facebook không được vượt quá 255 ký tự." });
            }


            // Kiểm tra số điện thoại (phải có 10 ký tự)
            if (!Regex.IsMatch(websiteInfo.WebsitePhone, @"^\d{10}$"))
            {
                return Ok(new { success = false, message = "Số điện thoại phải có đúng 10 ký tự." });
            }

            // Kiểm tra email (tối đa 255 ký tự và định dạng hợp lệ)
            if (!Regex.IsMatch(websiteInfo.WebsiteEmail, @"^[^@]+@[^@]+\.[^@]+$"))
            {
                return Ok(new { success = false, message = "Email không hợp lệ hoặc dài hơn 255 ký tự." });
            }

            var existingWebsite = await _context.Websites.FirstOrDefaultAsync(w => w.WebsiteId == 2);
            if (existingWebsite == null)
            {
                return NotFound("Website không tồn tại.");
            }

            // Cập nhật thông tin website
            existingWebsite.WebsiteName = websiteInfo.WebsiteName;
            existingWebsite.WebsiteAddress = websiteInfo.WebsiteAddress;
            existingWebsite.WebsiteLinkFb = websiteInfo.WebsiteLinkFb;
            existingWebsite.WebsitePhone = websiteInfo.WebsitePhone;
            existingWebsite.WebsiteEmail = websiteInfo.WebsiteEmail;

            // Nếu có ảnh logo mới, cập nhật đường dẫn hình ảnh
            if (!string.IsNullOrEmpty(websiteInfo.WebsiteImage))
            {
                existingWebsite.WebsiteImage = websiteInfo.WebsiteImage;
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.Websites.Update(existingWebsite);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Cập nhật thông tin website thành công." });
        }



    }
}
