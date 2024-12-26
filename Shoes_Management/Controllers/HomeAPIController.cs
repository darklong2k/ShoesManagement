using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        [HttpGet("LayOut")]
        public IActionResult LayOut()
        {
            var cate = _context.Categories.Take(2);
            var brand = _context.Brands;
            var website = _context.Websites.First();
            return Ok(new {cate,brand,website });
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
                              join o in _context.Orders on od.OrderId equals o.OrderId
                              where o.Status == "Delivered"
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
