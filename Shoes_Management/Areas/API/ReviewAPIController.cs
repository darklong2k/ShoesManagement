using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shoes_Management.Models;

namespace Shoes_Management.Areas.API
{
    
    [Route("api/reviews")]
    [ApiController]
    public class ReviewAPIController : ControllerBase
    {
        private readonly Shoescontext _shoescontext;
        public ReviewAPIController(Shoescontext shoescontext)
        {
            _shoescontext = shoescontext;
        }
        // API lấy reviews
        [HttpGet]
        public IActionResult GetReview()
        {
            // Kết bảng ProductDetails, Customers, Colors, Sizes, Reviews
            var query = from c in _shoescontext.Customers
                        join r in _shoescontext.Reviews on c.CustomerId equals r.CustomerId
                        join pd in _shoescontext.ProductDetails on r.ProductDetailId equals pd.ProductDetailId
                        join p in _shoescontext.Products on pd.ProductId equals p.ProductId
                        join co in _shoescontext.Colors on pd.ColorId equals co.ColorId
                        join s in _shoescontext.Sizes on pd.SizeId equals s.SizeId
                        select new
                        {
                            CustomerId = c.CustomerId,
                            CustomerName = c.Name,
                            Comment = r.Comment,
                            Rating = r.Rating,
                            ReviewStatus = r.Status,
                            ProductName = p.Name,
                            ColorName = co.ColorName,
                            SizeName = s.SizeName,
                            ProductDetailId = pd.ProductDetailId
                        };

            var reviews = query.ToList();

            if (reviews == null) return BadRequest();

            return Ok(reviews);
        }
        // Lấy review bằng id product detail và id customer
        public Review GetReviewByProductDetailIdAndCustomerId(int product_detail_id, int customer_id)
        {
            var reviews = _shoescontext.Reviews.ToList();
            var review = reviews.FirstOrDefault(review => review.CustomerId == customer_id && review.ProductDetailId == product_detail_id);
            return review;
        }

        // Chỉnh sửa review bằng id product detail và id customer
        [HttpPatch("{product_detail_id}/{customer_id}/{status}")]
        public IActionResult ChangeStatusReview(int product_detail_id, int customer_id, string status) 
        {
            var review = GetReviewByProductDetailIdAndCustomerId(product_detail_id, customer_id);
            
            if (review == null) return BadRequest();

            review.Status = status;
            try
            {
                // Save changes to the database
                _shoescontext.SaveChanges();
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during saving
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
            return Ok(review);
        }
    }
}
