using MCCConstruction.Data;
using MCCConstruction.Models;
using Microsoft.AspNetCore.Mvc;

namespace MccConstruction.Controllers
{
    public class QuoteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public QuoteController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /Quote
        [HttpGet]
        public IActionResult Index()
        {
            return View(new QuoteRequest());
        }

        // POST: /Quote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            QuoteRequest quoteRequest,
            IFormFile? attachment)
        {
            // Validate the uploaded file before saving anything
            if (attachment != null && attachment.Length > 0)
            {
                var allowedExtensions = new[]
                {
                    ".pdf",
                    ".doc",
                    ".docx",
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

                var extension = Path
                    .GetExtension(attachment.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "AttachmentPath",
                        "Only PDF, Word, JPG and PNG files are allowed.");

                    return View(quoteRequest);
                }

                // Maximum 10 MB
                if (attachment.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "AttachmentPath",
                        "The attachment must be smaller than 10 MB.");

                    return View(quoteRequest);
                }
            }

            // Stop if the form itself is invalid
            if (!ModelState.IsValid)
            {
                return View(quoteRequest);
            }

            string? savedFilePath = null;

            try
            {
                // ==========================================
                // SAVE ATTACHMENT
                // ==========================================

                if (attachment != null && attachment.Length > 0)
                {
                    var extension = Path
                        .GetExtension(attachment.FileName)
                        .ToLowerInvariant();

                    var uploadFolder = Path.Combine(
                        _environment.WebRootPath,
                        "uploads",
                        "quotes");

                    Directory.CreateDirectory(uploadFolder);

                    // Generate a safe unique filename
                    var safeFileName =
                        $"{Guid.NewGuid():N}{extension}";

                    savedFilePath = Path.Combine(
                        uploadFolder,
                        safeFileName);

                    await using (var stream = new FileStream(
                        savedFilePath,
                        FileMode.CreateNew))
                    {
                        await attachment.CopyToAsync(stream);
                    }

                    quoteRequest.AttachmentPath =
                        $"/uploads/quotes/{safeFileName}";
                }
                else
                {
                    quoteRequest.AttachmentPath = string.Empty;
                }

                // ==========================================
                // SET SYSTEM VALUES
                // ==========================================

                quoteRequest.DateSubmitted = DateTime.Now;
                quoteRequest.Status = "New";

                // ==========================================
                // SAVE TO DATABASE
                // ==========================================

                _context.QuoteRequests.Add(quoteRequest);

                await _context.SaveChangesAsync();

                // ==========================================
                // SUCCESS
                // ==========================================

                TempData["SuccessMessage"] =
                    "Your quote request has been submitted successfully. " +
                    "Our team will contact you shortly.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                // Delete uploaded file if database saving fails
                if (savedFilePath != null &&
                    System.IO.File.Exists(savedFilePath))
                {
                    System.IO.File.Delete(savedFilePath);
                }

                ModelState.AddModelError(
                    "",
                    "We could not submit your quote request. " +
                    "Please try again.");

                return View(quoteRequest);
            }
        }
    }
}