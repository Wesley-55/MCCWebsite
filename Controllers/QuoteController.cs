using MCCConstruction.Data;
using MCCConstruction.Models;
using Microsoft.AspNetCore.Mvc;

namespace MCCConstruction.Controllers
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
            // ==========================================
            // VALIDATE ATTACHMENT
            // ==========================================

            if (attachment != null && attachment.Length > 0)
            {
                const long maximumFileSize = 10 * 1024 * 1024;

                var allowedExtensions = new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
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

                // Validate extension
                if (string.IsNullOrWhiteSpace(extension) ||
                    !allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "AttachmentPath",
                        "Only PDF, Word, JPG and PNG files are allowed.");

                    return View(quoteRequest);
                }

                // Validate file size
                if (attachment.Length > maximumFileSize)
                {
                    ModelState.AddModelError(
                        "AttachmentPath",
                        "The attachment must be smaller than 10 MB.");

                    return View(quoteRequest);
                }

                // Validate that the file has a filename
                if (string.IsNullOrWhiteSpace(attachment.FileName))
                {
                    ModelState.AddModelError(
                        "AttachmentPath",
                        "The selected file is invalid.");

                    return View(quoteRequest);
                }
            }

            // ==========================================
            // VALIDATE FORM
            // ==========================================

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

                    // Never use the original filename.
                    // Generate a unique server-side filename instead.
                    var safeFileName =
                        $"{Guid.NewGuid():N}{extension}";

                    savedFilePath = Path.Combine(
                        uploadFolder,
                        safeFileName);

                    await using (var stream = new FileStream(
                        savedFilePath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None))
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
                // SAVE DATABASE RECORD
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
                // If the database operation fails after the
                // file was uploaded, remove the orphaned file.
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