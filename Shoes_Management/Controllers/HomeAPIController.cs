using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shoes_Management.Models;

namespace Shoes_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeAPIController : ControllerBase
    {
        private readonly Shoescontext _context;

        public HomeAPIController(Shoescontext context) {
            _context = context;
        }

        public IActionResult LayOut()
        {
            var cate = _context.Categories;
            return Ok(cate);
        }

        [HttpGet("GetProducts")]
        public IActionResult GetProducts()
        {
            //PRoduct new
            var products = _context.Products.Take(4).OrderByDescending(p => p.CreatedAt);
            //Product Best seller
            var bestSeller = (from pd in _context.ProductDetails
                              join od in _context.OrderDetails on pd.ProductDetailId equals od.ProductDetailId
                              join p in _context.Products on pd.ProductId equals p.ProductId
                              group od by p into grouped
                              select new
                              {
                                  Products = grouped.Key,
                                  ToatalQuanTiTY = grouped.Sum(od => od.Quantity)
                              }).OrderByDescending(x => x.ToatalQuanTiTY).Take(4);
            return Ok(new { products, bestSeller });
        }
    }
}
