using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
using Shoes_Management.Models;

namespace Shoes_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
	[ServiceFilter(typeof(AdminAuthorizationFilter))]
    public class HomeController : Controller
    {
        private readonly Shoescontext _context;

        public HomeController(Shoescontext context)
        {
            _context = context;
        }
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

        public IActionResult Comment_Page()
		{
			return View();
		}
		public IActionResult Product_Page()
		{
			return View();
		}
		public IActionResult Customer_Page()
		{
			return View();
		}
		public IActionResult Account_Page() 
		{
			return View();
		}
		public IActionResult Contact_Page()
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
        [HttpGet]
        public IActionResult Edit_Product_Page(int productId)
        {
            // Lấy thông tin sản phẩm từ cơ sở dữ liệu dựa trên productId
            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound();
            }

            // Trả về view với thông tin sản phẩm
            return View(product);
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


