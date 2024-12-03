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
