using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shoes_Management.Models;

namespace Shoes_Management.Areas.API
{
    [Route("api/contacts")]
    [ApiController]
    public class ContactAPIController : ControllerBase
    {
        private readonly Shoescontext _shoescontext;
        public ContactAPIController(Shoescontext shoescontext)
        {
            _shoescontext = shoescontext;
        }
        
        // API lấy contact
        [HttpGet]
        public IActionResult GetContact()
        {
            var contacts = _shoescontext.Contacts.ToList();
            if (contacts == null) return BadRequest();

            return Ok(contacts);
        }

        // Lấy contact bằng id contact
        public Contact GetContactById(int contactId)
        {
            try
            {
                var contact = _shoescontext.Contacts.FirstOrDefault(contact => contact.ContactId == contactId);

                if (contact == null)
                {
                    throw new Exception("Contact not found");
                }

                return contact;
            }
            catch (Exception ex)
            {
                
                throw new Exception("Lỗi ở GetContactById", ex);
            }
        }

        // Chỉnh sửa status contact bằng id contact
        [HttpPatch("{contactId}")]
        public IActionResult ChangeContactStatus(int contactId)
        {
            Contact contact = GetContactById(contactId);
            if (contact == null)
            {
                return NotFound();
            }

            contact.ContactStatus = "Resolved";
            try
            {
                _shoescontext.SaveChanges();
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during saving
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

            return Ok(contact);
        }

        // Xóa contact
        [HttpDelete("{contactId}")]
        public IActionResult DeleteContact(int contactId)
        {
            Contact contact = GetContactById(contactId);
            if (contact == null)
            {
                return NotFound();
            }

            try
            {
                _shoescontext.Contacts.Remove(contact);
                _shoescontext.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

            return Ok("Đã xóa contact");
        }

        // Tạo contact
        [HttpPost]
        public IActionResult CreateContact([FromBody] Contact contact)
        {
            _shoescontext.Contacts.Add(contact);
            _shoescontext.SaveChanges();
            return Ok(contact);
        }
    }
}
