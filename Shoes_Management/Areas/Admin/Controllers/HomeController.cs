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

		
		public IActionResult Comment_Page()
		{
			return View();
		}
		public IActionResult Product_Page()
		{
			return View();
		}
		public IActionResult Dashboard()
		{
			return View();
		}
		public IActionResult Statistics()
		{ 
			return View(); 

		}
		public IActionResult Add_Product_Page()
		{
			return View();
		}
		public IActionResult Edit_Product_Page()
		{
			return View();
		}
		public IActionResult Details_Product_Page()
		{

			return View();
		}
		public IActionResult NhapHang_Page()
		{
			return View();
		}
	}
}
