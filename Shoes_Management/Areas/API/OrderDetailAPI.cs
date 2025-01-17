using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Shoes_Management.Models;
using static NuGet.Packaging.PackagingConstants;
using Microsoft.EntityFrameworkCore;

namespace Shoes_Management.Areas.API
{
    [Route("api/orderdetails")]
    [ApiController]
    public class OrderDetailAPI : ControllerBase
    {
        private readonly Shoescontext _shoescontext;
        public OrderDetailAPI(Shoescontext shoescontext)
        {
            _shoescontext = shoescontext;
        }
        [HttpGet]
        public IActionResult GetOrderDetail()
        {
            var orders = _shoescontext.Orders;
            var orderDetails = _shoescontext.OrderDetails;
            var customers = _shoescontext.Customers;
            var payments = _shoescontext.Payments;

            var result = (from o in orders
                          join od in orderDetails on o.OrderId equals od.OrderId
                          join c in customers on o.CustomerId equals c.CustomerId
                          join p in payments on o.OrderId equals p.OrderId
                          group new { o, od, c, p } by new
                          {
                              o.OrderId,
                              o.OrderDate,
                              c.Name,
                              c.Phone,
                              o.Status,
                              PaymentStatus = p.Status,
                              o.TotalAmount
                          } into groupedData
                          select new
                          {
                              groupedData.Key.OrderId,
                              groupedData.Key.OrderDate,
                              CustomerName = groupedData.Key.Name,
                              CustomerPhone = groupedData.Key.Phone,
                              groupedData.Key.Status,
                              groupedData.Key.PaymentStatus,
                              groupedData.Key.TotalAmount
                          })
              .OrderBy(x => x.OrderId)
              .ToList();

            return Ok(result);
        }
        public Order GetOrderById(int orderId)
        {
            var orders = _shoescontext.Orders.ToList();

            var findedOrder = orders.FirstOrDefault(order => order.OrderId == orderId);
            return findedOrder;
        }
        [HttpPatch("{orderId}")]
        public IActionResult ChangeStatusOrder(int orderId) 
        {
            var order = GetOrderById(orderId);
            switch(order.Status)
            {
                case "Pending":
                    order.Status = "Packing";
                    break;
                case "Packing":
                    order.Status = "Shipping";
                    break;
                case "Shipping":
                    order.Status = "Delivered";
                    break;
            }

            _shoescontext.SaveChanges();
            return Ok(order);
        }

        [HttpDelete("{orderId}")]
        public IActionResult CancelOrder(int orderId)
        {
            var order = GetOrderById(orderId);

            order.Status = "Canceled";

            _shoescontext.SaveChanges();

            return Ok(order);
        }
    }
}
