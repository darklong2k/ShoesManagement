using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shoes_Management.Models;

namespace Shoes_Management.Areas.API
{
    [Route("api/suppliers")]
    [ApiController]
    public class SupplierAPIController : ControllerBase
    {
        private Shoescontext _shoescontext;
        public SupplierAPIController(Shoescontext shoescontext)
        {
            _shoescontext = shoescontext;
        }
        [HttpPost]
        public IActionResult CreateSupplier([FromBody] Supplier supplier) 
        {
            if (supplier == null)
            {
                return BadRequest("Invalid supplier data.");
            }
            _shoescontext.Suppliers.Add(supplier);
            _shoescontext.SaveChanges();
            return Ok(new { data = supplier });
        }
        [HttpGet]
        public IActionResult GetListSuppliers()
        {
            return Ok(new {data = _shoescontext.Suppliers.ToList()});
        }

        [HttpGet("{SupplierId}")]
        public Supplier GetSupplierById(int SupplierId)
        {
            return _shoescontext.Suppliers.FirstOrDefault(supplier => supplier.SupplierId == SupplierId);
        }

        [HttpPut("{SupplierId}")]
        public IActionResult ChangeSupplier(int SupplierId, [FromBody] Supplier supplier)
        {
            try
            {
                var existingSupplier = _shoescontext.Suppliers.FirstOrDefault(s => s.SupplierId == SupplierId);

                if (existingSupplier == null)
                {
                    return NotFound("Nhà cung cấp không tồn tại");
                }

                // Cập nhật các thông tin
                existingSupplier.SupplierName = supplier.SupplierName;
                existingSupplier.Address = supplier.Address;
                existingSupplier.Phone = supplier.Phone;
                existingSupplier.Email = supplier.Email;
                existingSupplier.Status = supplier.Status;
                _shoescontext.SaveChanges();
                return Ok(new { data = supplier });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        [HttpDelete("{SupplierId}")]
        public IActionResult DeleteSupplier(int SupplierId)
        {
            Supplier supplier = GetSupplierById(SupplierId);
            if (supplier.Status == true)
            {
                supplier.Status = false;
            } 

            _shoescontext.SaveChanges();

            return Ok(new { data = supplier });
        }



    }
}
