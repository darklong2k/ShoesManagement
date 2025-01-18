using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Models;
using System.Diagnostics;

namespace Shoes_Management.Controllers
{
	
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly Shoescontext _context;

        public HomeController(ILogger<HomeController> logger, Shoescontext context)
		{
			_logger = logger;
            _context = context;
        }

		public IActionResult Index()
		{
			return View();
		}
        public IActionResult HoanThanhDonHang()
        {
            return View();
        }
        public IActionResult TrangThongTinGiaoHang()
        {
            return View();
        }
        public IActionResult TrangThanhToan()
        {
            return View();
        }

        public IActionResult Privacy()
		{
			return View();
		}

		public IActionResult TrangChu()
		{
			return View();
		}

		public IActionResult TrangSanPham()
		{
			return View();
		}
        public IActionResult TrangMoMo()
        {
            return View();
        }
        public IActionResult TrangGioiThieu()
		{
			return View();
		}

        [Route("Home/TrangChiTietSP/{id:int}")]
        public IActionResult RedirectToSlug(int id)
        {
            // Tìm sản phẩm theo ID
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound(); // Nếu không tìm thấy sản phẩm
            }

            // Chuyển hướng đến URL có slug
            return RedirectToAction("TrangChiTietSP", new { slug = product.Slug });
        }

        // Route để hiển thị trang chi tiết sản phẩm qua slug
        [Route("Home/TrangChiTietSP/{slug}")]
        public IActionResult TrangChiTietSP(string slug)
        {
            // Tìm sản phẩm theo slug
            var product = _context.Products.FirstOrDefault(p => p.Slug == slug);

            if (product == null)
            {
                return NotFound(); // Nếu không tìm thấy sản phẩm
            }
            ViewData["ProductId"] = product.ProductId;

            // Trả về view chi tiết sản phẩm
            return View(product);
        }

        public IActionResult TrangGioHang()
		{
			return View();
		}

		public IActionResult TrangLienHe()
		{
			return View();
		}

		public IActionResult TrangCaNhan()
		{
			return View();
		}
        public IActionResult TrangBlog()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
		public IActionResult Contact()
		{
			return View();
		}
	}
}
