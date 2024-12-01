using Microsoft.AspNetCore.Mvc;

namespace Shoes_Management.Areas.Admin.Controllers
{
	public class HomeController : Controller
	{
		[Area("Admin")]
		public IActionResult Index()
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
	}
}
