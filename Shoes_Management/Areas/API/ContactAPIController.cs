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

                return contact; // Return the found contact
            }
            catch (Exception ex)
            {
                
                throw new Exception("An error occurred while retrieving the contact", ex);
            }
        }

        // Chỉnh sửa status contact bằng id contact
        [HttpPatch("{contactId}")]
        public IActionResult ChangeContactStatus(int contactId)
        {
            Contact contact = GetContactById(contactId);
            if (contact == null)
            {
                return NotFound(); // Return a 404 if the contact does not exist
            }

            contact.ContactStatus = "Resolved";
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

            return Ok(contact);
        }

        // Xóa contact
        [HttpDelete("{contactId}")]
        public IActionResult DeleteContact(int contactId)
        {
            Contact contact = GetContactById(contactId);
            if (contact == null)
            {
                return NotFound(); // Return a 404 if the contact does not exist
            }

            try
            {
                _shoescontext.Contacts.Remove(contact);
                // Save changes to the database
                _shoescontext.SaveChanges();
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during saving
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

            return Ok(contact);
        }

        // Tạo contact
        [HttpPost]
        public IActionResult CreateContact([FromBody] Contact contact)
        {
            _shoescontext.Add(contact);
            _shoescontext.SaveChanges();
            return Ok(contact);
        }
    }
}
