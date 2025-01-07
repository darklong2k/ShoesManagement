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
            var accounts = _shoescontext.Accounts.ToList();

            var result = from c in customers
                         join a in accounts on c.AccountId equals a.AccountId
                         select new
                         {
                             CustomerId = c.CustomerId,
                             Name = c.Name,
                             Phone = c.Phone,
                             Sex = c.Sex,
                             Email = c.Email,
                             Address = c.Address,
                             AccountId = c.AccountId,
                             Username = a.Username
                         };
            var customerWithAccount = result.ToList();

            if (customers == null)
            {
                return BadRequest(new {message = "Don't have any customers"});
            }
            return Ok(customerWithAccount);
        }
    }
}
