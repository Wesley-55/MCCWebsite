using MCCConstruction.Data;
using MCCConstruction.Models;
using Microsoft.AspNetCore.Mvc;

namespace MCCConstruction.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Contact
        [HttpGet]
        public IActionResult Index()
        {
            return View(new ContactMessage());
        }

        // POST: /Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactMessage contactMessage)
        {
            if (!ModelState.IsValid)
            {
                return View(contactMessage);
            }

            contactMessage.DateSent = DateTime.Now;
            contactMessage.Status = "New";

            _context.ContactMessages.Add(contactMessage);

            await _context.SaveChangesAsync();

            TempData["ContactSuccess"] =
                "Thank you for contacting MCC Construction. Your message has been received and our team will get back to you shortly.";

            return RedirectToAction(nameof(Index));
        }
    }
}