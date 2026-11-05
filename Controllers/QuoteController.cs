using MCCConstruction.Data;
using MCCConstruction.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace MCCConstruction.Controllers
{
    public class QuoteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<QuoteController> _logger;

        public QuoteController(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<QuoteController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
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
            string? uploadedObjectPath = null;

            // ==========================================
            // VALIDATE ATTACHMENT
            // ==========================================

            var allowedContentTypes = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase)
            {
                { ".pdf", "application/pdf" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" },
                { ".webp", "image/webp" }
            };

            if (attachment != null && attachment.Length > 0)
            {
                const long maximumFileSize = 10 * 1024 * 1024;

                var extension = Path.GetExtension(attachment.FileName);

                if (string.IsNullOrWhiteSpace(extension) ||
                    !allowedContentTypes.ContainsKey(extension))
                {
                    ModelState.AddModelError(
                        "AttachmentPath",
                        "Only PDF, JPG, PNG and WEBP files are allowed.");
                }

                if (attachment.Length > maximumFileSize)
                {
                    ModelState.AddModelError(
                        "AttachmentPath",
                        "The attachment must not exceed 10 MB.");
                }

                if (string.IsNullOrWhiteSpace(attachment.FileName))
                {
                    ModelState.AddModelError(
                        "AttachmentPath",
                        "The selected file is invalid.");
                }
            }

            // ==========================================
            // VALIDATE FORM
            // ==========================================

            if (!ModelState.IsValid)
            {
                return View(quoteRequest);
            }

            try
            {
                // ==========================================
                // UPLOAD ATTACHMENT TO SUPABASE
                // ==========================================

                if (attachment != null && attachment.Length > 0)
                {
                    var supabaseUrl = _configuration["Supabase:Url"];
                    var bucket = _configuration["Supabase:Bucket"];
                    var secretKey = _configuration["Supabase:ServiceRoleKey"];

                    if (string.IsNullOrWhiteSpace(supabaseUrl) ||
                        string.IsNullOrWhiteSpace(bucket) ||
                        string.IsNullOrWhiteSpace(secretKey))
                    {
                        throw new InvalidOperationException(
                            "Supabase Storage configuration is missing.");
                    }

                    var extension = Path.GetExtension(attachment.FileName);

                    var contentType = allowedContentTypes[extension];

                    // Generate a unique filename.
                    var safeFileName =
                        $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

                    var folder = Guid.NewGuid().ToString("N");

                    uploadedObjectPath = $"{folder}/{safeFileName}";

                    var uploadUrl =
                        $"{supabaseUrl.TrimEnd('/')}/storage/v1/object/" +
                        $"{Uri.EscapeDataString(bucket)}/" +
                        $"{uploadedObjectPath}";

                    var httpClient = _httpClientFactory.CreateClient();

                    using var uploadRequest = new HttpRequestMessage(
                        HttpMethod.Post,
                        uploadUrl);

                    uploadRequest.Headers.Add("apikey", secretKey);

                    uploadRequest.Headers.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer", secretKey);

                    uploadRequest.Headers.Add("x-upsert", "false");

                    var fileContent = new StreamContent(
                        attachment.OpenReadStream());

                    fileContent.Headers.ContentType =
                        new MediaTypeHeaderValue(contentType);

                    uploadRequest.Content = fileContent;

                    using var uploadResponse =
                        await httpClient.SendAsync(uploadRequest);

                    if (!uploadResponse.IsSuccessStatusCode)
                    {
                        var error = await uploadResponse.Content.ReadAsStringAsync();

                        _logger.LogError(
                            "Supabase upload failed. Status: {StatusCode}. Response: {Response}",
                            uploadResponse.StatusCode,
                            error);

                        throw new Exception(
                            "The attachment could not be uploaded.");
                    }

                    // Save the storage path in PostgreSQL.
                    quoteRequest.AttachmentPath = uploadedObjectPath;
                }
                else
                {
                    quoteRequest.AttachmentPath = string.Empty;
                }

                // ==========================================
                // SET SYSTEM VALUES
                // ==========================================

                quoteRequest.DateSubmitted = DateTime.UtcNow;
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
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to submit a quote request.");

                // ==========================================
                // DELETE UPLOADED FILE IF DATABASE FAILS
                // ==========================================

                if (!string.IsNullOrWhiteSpace(uploadedObjectPath))
                {
                    try
                    {
                        var supabaseUrl = _configuration["Supabase:Url"];
                        var bucket = _configuration["Supabase:Bucket"];
                        var secretKey = _configuration["Supabase:ServiceRoleKey"];

                        if (!string.IsNullOrWhiteSpace(supabaseUrl) &&
                            !string.IsNullOrWhiteSpace(bucket) &&
                            !string.IsNullOrWhiteSpace(secretKey))
                        {
                            var deleteUrl =
                                $"{supabaseUrl.TrimEnd('/')}/storage/v1/object/" +
                                $"{Uri.EscapeDataString(bucket)}/" +
                                $"{uploadedObjectPath}";

                            var httpClient = _httpClientFactory.CreateClient();

                            using var deleteRequest = new HttpRequestMessage(
                                HttpMethod.Delete,
                                deleteUrl);

                            deleteRequest.Headers.Add("apikey", secretKey);

                            deleteRequest.Headers.Authorization =
                                new AuthenticationHeaderValue(
                                    "Bearer", secretKey);

                            using var deleteResponse =
                                await httpClient.SendAsync(deleteRequest);

                            if (!deleteResponse.IsSuccessStatusCode)
                            {
                                _logger.LogWarning(
                                    "Could not delete orphaned Supabase attachment.");
                            }
                        }
                    }
                    catch (Exception cleanupException)
                    {
                        _logger.LogError(
                            cleanupException,
                            "Failed to clean up uploaded attachment.");
                    }
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