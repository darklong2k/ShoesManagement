using Microsoft.AspNetCore.Mvc;
using Shoes_Management.Models;
using System.Diagnostics;

namespace Shoes_Management.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
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

		public IActionResult TrangChiTietSP()
		{
			return View();
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
