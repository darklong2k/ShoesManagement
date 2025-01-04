using Microsoft.AspNetCore.Mvc;

namespace Shoes_Management.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class SupplierController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TrangQuanLyNCC()
        {
            return View();
        }
    }
}
