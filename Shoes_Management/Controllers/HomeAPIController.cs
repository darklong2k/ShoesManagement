using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Shoes_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeAPIController : ControllerBase
    {
        private readonly Shoescontext _context;

        public string HashPassWord(string password)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        public HomeAPIController(Shoescontext context)
        {
            _context = context;
        }

        [HttpGet("DangNhap")]
        public IActionResult DangNhap()
        {
            var acc = HttpContext.Session.GetString("username");
            var isAdmin = HttpContext.Session.GetString("is_admin");
            if (string.IsNullOrEmpty(acc))
            {
                return Ok(new { success = false });
            }
            if (isAdmin != null)
            {
                return Ok(new { isAdmin, url = "/Admin/Home/Dashboard" });
            }
            else
            {
                return Ok(new { success = true, url = "/home/trangcanhan" });
            }
        }

        [HttpPost("DangNhap")]
        public IActionResult DangNhap([FromForm] string username, [FromForm] string password)//PhucNguyen2004
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Ok(new { success = false, message = "Mật khẩu và xác nhận mật khẩu không khớp" });
            }
            else
            {
                var acc = _context.Accounts.FirstOrDefault(a => a.Username == username && a.Password == HashPassWord(password));

                if (acc == null)
                {
                    return Ok(new { success = false, message = "Tài khoản và mật khẩu không hợp lệ" });
                }
                else
                {
                    HttpContext.Session.SetString("username", acc.Username);
                    if (acc.IsAdmin == true)
                    {
                        HttpContext.Session.SetString("is_admin", acc.IsAdmin.ToString());
                        return Ok(new { admin = true, url = "/Admin/Home/Dashboard" });
                    }
                    HttpContext.Session.SetString("acc_id", acc.AccountId.ToString());
                    return Ok(new { success = true, url = "/home/trangcanhan" });
                }

            }
        }

        [HttpGet("TrangCaNhan")]
        public IActionResult TrangCaNhan()
        {
            var acc_id = HttpContext.Session.GetString("acc_id");
            var username = HttpContext.Session.GetString("username");
            var custumerInFo = _context.Customers.FirstOrDefault(c => c.AccountId.ToString() == acc_id);
            return Ok(new { success = true, custumerInFo, username, acc_id });
        }

        [HttpGet("DangXuat")]
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return Ok(new { success = true, url = "/Home/trangchu" });
        }

        [HttpGet("LayOut")]
        public IActionResult LayOut()
        {
            var accName = HttpContext.Session.GetString("username");
            var cate = _context.Categories.Take(2);
            var brand = _context.Brands;
            var website = _context.Websites.First();
            return Ok(new { cate, brand, website, accName });
        }

        //Trang chủ
        [HttpGet("GetProducts")]
        public IActionResult GetProducts()
        {
            //PRoduct new
            var products = _context.Products.Take(4).OrderByDescending(p => p.CreatedAt);
            //Product Best seller
            var bestSeller = _context.OrderDetails
                .Where(od => od.Order.Status == "Delivered")
                .GroupBy(od => od.ProductDetail.Product)
                .Select(grouped => new
                {
                    Product = grouped.Key,
                    TotalQuantity = grouped.Sum(od => od.Quantity)
                })
                .OrderByDescending(od => od.TotalQuantity)
                .Take(4);
            return Ok(new { products, bestSeller });
        }

        //Hiện categories nhỏ cho trang chủ
        [HttpGet("GetCategories")]
        public IActionResult GetCategories()
        {
            var categories = _context.Categories.Skip(2);
            return Ok(categories);
        }

        //Lấy sản phẩm từ category danh mục nhỏ trong trang chủ
        [HttpGet("GetProductsByCategory/{categoryId}")]
        public IActionResult GetProductsByCategory(int categoryId)
        {
            var product = _context.Products.Where(p => p.CategoryId == categoryId);
            return Ok(product);
        }

        //TrangSanPham
        [HttpGet("Products_Page")]
        public IActionResult Products_Page(int page = 1, string search = null, int brandId = 0, int categoryId = 0, int priceId = 0)
        {
            int pageSize = 3;

            var query = _context.Products.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }
            if (categoryId != 0)
            {
                var categoryid = _context.Categories.Where(c => c.ParentId == categoryId).Select(c => c.CategoryId).ToList();
                query = query.Where(p => categoryid.Contains(p.CategoryId ?? 0));
            }
            if (brandId != 0)
            {
                query = query.Where(p => p.BrandId == brandId);
            }
            if (priceId != 0)
            {
                switch (priceId)
                {
                    case 1:
                        query = query.Where(p => p.Price < 2000000);
                        break;
                    case 2:
                        query = query.Where(p => p.Price >= 2000000 && p.Price < 3000000);
                        break;
                    case 3:
                        query = query.Where(p => p.Price >= 3000000 && p.Price < 4000000);
                        break;
                    case 4:
                        query = query.Where(p => p.Price >= 4000000);
                        break;
                }
            }
            var TotalProducts = query.Count();
            var products = query.Skip((page - 1) * pageSize).Take(pageSize);
            int totalPage = (int)Math.Ceiling((double)TotalProducts / pageSize);
            return Ok(new { currentPage = page, totalPage, pageSize, products });
        }

        //Hiện lên bộ lọc
        [HttpGet("TrangSanPham")]
        public IActionResult TrangSanPham()
        {
            var category = _context.Categories.Take(2);
            var brands = _context.Brands;
            return Ok(new { category = category, brands = brands });
        }

        //Lấy dsach cac bai blog và hiển thị giới thiệu shop
        [HttpGet("GetBlogs")]
        public IActionResult GetBlogs(int page = 1)
        {
            int pagesize = 2;
            var blogs = _context.Blogs.Skip((page - 1) * pagesize).Take(pagesize);
            var totalBlogs = _context.Blogs.Count();
            int totalPage = (int)Math.Ceiling((double)totalBlogs / pagesize);
            var website = _context.Websites.First();
            return Ok(new { blogs = blogs, currentPage = page, totalPage, pagesize, website });
        }

        //Hiện trang blog theo id
        [HttpGet("BlogId")]
        public IActionResult BlogId(int blogid)
        {
            var sidebarBlog = _context.Blogs.Include(b => b.BlogImages).Select(b => new
            {
                b.BlogId,
                b.BlogTitle,
                BlogImage = b.BlogImages.Select(img => img.ImageUrl)
            }).Where(b => b.BlogId != blogid);
            var blog = _context.Blogs
                .Include(b => b.BlogImages)
                .Where(b => b.BlogId == blogid)
                .Select(b => new
                {
                    b.BlogId,
                    b.BlogTitle,
                    b.BlogContent,
                    BlogImages = b.BlogImages.Select(img => img.ImageUrl).ToList()
                })
                .FirstOrDefault(b => b.BlogId == blogid);

            return Ok(new { blog = blog, sidebarBlog });
        }
        [HttpPost("ChangePassWord/{$id}")]
        public IActionResult ChangePassWord([FromForm] int acc_id, [FromForm] string CurrentPassword, [FromForm] string newPassword)
        {
            // Lấy thông tin tài khoản từ session hoặc database
          
            var acc = _context.Accounts.FirstOrDefault(a => a.AccountId == acc_id);
            Console.WriteLine(acc);
            if (acc == null)
            {
                return Ok(new { success = false, message = "Tài khoản không tồn tại" });
            }

            // Kiểm tra mật khẩu hiện tại
            if (acc.Password != HashPassWord(CurrentPassword))
            {
                return Ok(new { success = false, message = "Mật khẩu hiện tại không đúng" });
            }

            // Cập nhật mật khẩu mới
            acc.Password = HashPassWord(newPassword);
            _context.SaveChanges();

            return Ok(new { success = true, message = "Mật khẩu đã được thay đổi thành công." });
        }

   


        [HttpPost("DangKy")]
        public IActionResult DangKy([FromForm]string username, [FromForm] string password, [FromForm]string passwordConfirm)
        {
            if (password != passwordConfirm)
            {
                return Ok(new { success = false, message = "Mật khẩu và xác nhận mật khẩu không khớp" });
            }
            if (_context.Accounts.Any(a => a.Username == username))
            {
                return Ok(new {success=false,message = "Tài khoản đã tồn tại"});
            }
            var acc = new Account();
            acc.Username = username;
            acc.Password = HashPassWord(passwordConfirm);
            acc.IsAdmin = false;
            acc.Status = "Active";
            acc.CreatedAt = DateTime.UtcNow;
            acc.UpdatedAt = DateTime.UtcNow;
            _context.Accounts.Add(acc);
            _context.SaveChanges();
            return Ok(new {success=true});
        }
        
        [HttpGet("Admin")]
        public IActionResult Admin()
        {
            var nameAdmin = HttpContext.Session.GetString("username");
            return Ok(nameAdmin);
        }
    }
}