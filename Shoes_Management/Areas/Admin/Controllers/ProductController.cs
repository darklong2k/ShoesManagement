using Microsoft.AspNetCore.Mvc;

namespace Shoes_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
