using Microsoft.AspNetCore.Mvc;

namespace Shoes_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
		public IActionResult Index()
		{
			return View();
		}

        public IActionResult TrangDanhMucSanPham()
        {
            return View();
        }

        public IActionResult TrangQuanLyDonHang()
        {
            return View();
        }

        public IActionResult TrangQuanLyNCC()
        {
            return View();
        }

        public IActionResult TrangChiTietDonHang()
        {
            return View();
        }
    }
}
