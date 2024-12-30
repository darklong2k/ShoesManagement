using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shoes_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeAPIController : ControllerBase
    {
        private readonly Shoescontext _context;

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
            if (!string.IsNullOrEmpty(isAdmin))
            {
                return Ok(new { isAdmin, url = "/Admin/Home/Dashboard" });
            }
            else
            {
                return Ok(new { success = true, url = "/home/trangcanhan" });
            }
        }

        [HttpPost("DangNhap")]
        public IActionResult DangNhap([FromForm] string username, [FromForm] string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Ok(new { success = false });
            }
            else
            {
                var acc = _context.Accounts.FirstOrDefault(a => a.Username == username && a.Password == password);
                HttpContext.Session.SetString("username", acc.Username);
                if (acc == null)
                {
                    return Ok(new { success = false, message = "Tài khoản và mật khẩu không hợp lệ" });
                }
                if (acc.IsAdmin == true)
                {
                    HttpContext.Session.SetString("is_admin", acc.IsAdmin.ToString());
                    return Ok(new { admin = true, url = "/Admin/Home/Dashboard" });
                }
                HttpContext.Session.SetString("acc_id", acc.AccountId.ToString());
                return Ok(new { success = true, url = "/home/trangcanhan" });
            }
        }

        [HttpGet("TrangCaNhan")]
        public IActionResult TrangCaNhan()
        {
            var acc_id = HttpContext.Session.GetString("acc_id");
            var custumerInFo = _context.Customers.FirstOrDefault(c => c.AccountId.ToString() == acc_id);
            return Ok(new { success = true, custumerInFo });
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
            var cate = _context.Categories.Take(2);
            var brand = _context.Brands;
            var website = _context.Websites.First();
            return Ok(new { cate, brand, website });
        }


        //Trang chủ
        [HttpGet("GetProducts")]
        public IActionResult GetProducts()
        {
            //PRoduct new
            var products = _context.Products.Take(4).OrderByDescending(p => p.CreatedAt);
            //Product Best seller
            var bestSeller = (from pd in _context.ProductDetails
                              join od in _context.OrderDetails on pd.ProductDetailId equals od.ProductDetailId
                              join p in _context.Products on pd.ProductId equals p.ProductId
                              join o in _context.Orders on od.OrderId equals o.OrderId
                              where o.Status == "Delivered"
                              group od by p into grouped
                              select new
                              {
                                  Products = grouped.Key,
                                  ToatalQuanTiTY = grouped.Sum(od => od.Quantity)
                              }).OrderByDescending(x => x.ToatalQuanTiTY).Take(4);
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
        public IActionResult Products_Page(int page = 1, string search = null, int categoryId = 0, int brandId = 0,int priceId = 0)
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
                        query = query.Where(p => p.Price >= 2000000);
                        break;
                }
            }
            var TotalProducts = query.Count();
            var products = query.Skip((page - 1) * pageSize).Take(pageSize);
            int totalPage = (int)Math.Ceiling((double)TotalProducts / pageSize);
            return Ok(new { currentPage = page, totalPage, pageSize, products });
        }

        [HttpGet("TrangSanPham")]
        public IActionResult TrangSanPham()
        {
            var category = _context.Categories.Take(2);
            var brands = _context.Brands;
            return Ok(new { category = category, brands = brands });
        }
    }
}
