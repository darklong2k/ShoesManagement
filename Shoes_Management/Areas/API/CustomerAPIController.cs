using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shoes_Management.Models;

namespace Shoes_Management.Areas.API
{
    [Route("api/customers")]
    [ApiController]
    public class CustomerAPIController : ControllerBase
    {
        private readonly Shoescontext _shoescontext;
        public CustomerAPIController(Shoescontext shoescontext)
        {
            _shoescontext = shoescontext;
        }
        [HttpGet]
        public IActionResult GetCustomer()
        {
            var customers = _shoescontext.Customers.ToList();
            if (customers == null)
            {
                return BadRequest(new {message = "Don't have any customers"});
            }
            return Ok(_shoescontext.Customers.ToList());
        }
    }
}
