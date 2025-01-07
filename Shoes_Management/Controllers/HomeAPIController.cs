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
        public IActionResult DangNhap([FromForm] string username, [FromForm] string password)//PhucNguyen2004 caothang Caothang1
        {
            if (!Regex.IsMatch(username,@"^[a-z]{4,8}"))
            {
                return Ok(new { success = false, message = "Tên đăng nhập gồm chữ cái và độ dài tối thiểu 4 đến 8 ký tự."});
            }
            if (!Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*\d)[A-Za-z\d@$!%*?&]{6,}$"))
            {
                return Ok(new { success = false, message = "Mật khẩu phải chứa ít nhất một chữ hoa, một số và tối thiểu 6 ký tự." });
            }
            else
            {
                var acc = _context.Accounts.Where(a => a.Status == "Active")
                    .FirstOrDefault(a => a.Username == username && a.Password == HashPassWord(password));
                if (acc == null)
                {
                    return Ok(new { success = false, message = "Tài khoản và mật khẩu không chính xác" });
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
                    return Ok(new { success = true, url = "/home/trangchu" });
                }
            }
        }

        [HttpGet("TrangCaNhan")]
        public IActionResult TrangCaNhan()
        {
            var acc_id = HttpContext.Session.GetString("acc_id");
            var username = HttpContext.Session.GetString("username");
            var custumerInFo = _context.Customers.FirstOrDefault(c => c.AccountId.ToString() == acc_id);
            return Ok(new { success = true, custumerInFo,username,acc_id });
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
            return Ok(new { cate, brand, website,accName });
        }

        //Trang chủ
        [HttpGet("GetProducts")]
        public IActionResult GetProducts()
        {
            //PRoduct new
            var products = _context.Products.Take(4).OrderByDescending(p => p.CreatedAt).Where(p => p.Status == "Active");
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
                .Take(4)
                .Where(p => p.Product.Status == "Active");
            return Ok(new { products, bestSeller });
        }

        //Hiện categories nhỏ cho trang chủ
        [HttpGet("GetCategories")]
        public IActionResult GetCategories()
        {
            var categories = _context.Categories.Skip(2).Where(c => c.Status == true);
            return Ok(categories);
        }

        //Lấy sản phẩm từ category danh mục nhỏ trong trang chủ
        [HttpGet("GetProductsByCategory/{categoryId}")]
        public IActionResult GetProductsByCategory(int categoryId)
        {
            var product = _context.Products.Where(p => p.CategoryId == categoryId && p.Status == "Active");
            return Ok(product);
        }

        //TrangSanPham
        [HttpGet("Products_Page")]
        public IActionResult Products_Page(int page = 1, string search = null, int brandId = 0, string categorySlug = null,int priceId = 0)
        {
            int pageSize = 4;

            var query = _context.Products.Where(p => p.Status == "Active").AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }
            if (categorySlug != null)
            {
                var CategorySlug = _context.Categories.FirstOrDefault(c => c.slug == categorySlug);
                var categoryid = _context.Categories.Where(c => c.ParentId == CategorySlug.CategoryId).Select(c => c.CategoryId).ToList();
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
            return Ok(new { currentPage = page, totalPage, pageSize, products,TotalProducts });
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
        public IActionResult GetBlogs(int page=1)
        {
            int pagesize = 2;
            var blogs = _context.Blogs.Skip((page - 1)*pagesize).Take(pagesize);
            var totalBlogs = _context.Blogs.Count();
            int totalPage = (int)Math.Ceiling((double)totalBlogs / pagesize);
            var gioithieu = _context.Blogs.Find(4);
            return Ok(new { blogs = blogs, currentPage = page,totalPage,pagesize,gioithieu });
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
                .First();

            return Ok(new { blog = blog, sidebarBlog });
        }
        [HttpPost("ChangePassWord/{id}")]
        public IActionResult ChangePassWord(int id, [FromForm] string username, [FromForm] string password, [FromForm] string passwordConfirm)
        {
            // Lấy acc_id từ session
            var acc_id = HttpContext.Session.GetString("acc_id");

            // Kiểm tra xem tài khoản có tồn tại không
            var acc = _context.Accounts.FirstOrDefault(a => a.AccountId.ToString() == acc_id);
            if (acc == null)
            {
                return Ok(new { success = false, message = "Tài khoản không tồn tại hoặc không khớp với thông tin đăng nhập" });
            }

            // Kiểm tra mật khẩu hiện tại
            if (acc.Password != HashPassWord(password))  // Kiểm tra mật khẩu cũ đã được hash chưa
            {
                return Ok(new { success = false, message = "Mật khẩu hiện tại không đúng" });
            }

            // Kiểm tra mật khẩu mới và xác nhận mật khẩu có khớp không
            if (password != passwordConfirm)
            {
                return Ok(new { success = false, message = "Mật khẩu và xác nhận mật khẩu không khớp" });
            }

            // Cập nhật mật khẩu mới sau khi đã hash
            acc.Password = HashPassWord(passwordConfirm);  // Hash mật khẩu mới và lưu vào cơ sở dữ liệu
            acc.UpdatedAt = DateTime.UtcNow;

            // Lưu thay đổi vào cơ sở dữ liệu
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
            if (!Regex.IsMatch(username, @"^[a-zA]{4,8}"))
            {
                return Ok(new { success = false, message = "Tên đăng nhập tối thiểu chữ cái và độ dài 4 đến 8 ký tự." });
            }
            if (!Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*\d)[A-Za-z\d@$!%*?&]{6,}$"))
            {
                return Ok(new { success = false, message = "Mật khẩu phải chứa ít nhất một chữ hoa, một số và tối thiểu 6 ký tự." });
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
            var cus = new Customer();
            cus.AccountId = acc.AccountId;
            _context.Customers.Add(cus);
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