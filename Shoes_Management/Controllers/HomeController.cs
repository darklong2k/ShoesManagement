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
        public IActionResult ThongTinGiaoHang()
        {
            return View();
        }
        public IActionResult DangNhapOrDangKi()
        {
            return View();
        }

        public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
