using System.Security.Claims;
using MCCConstruction.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MccConstruction.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ------------------------------------------------------------
        // ADMIN LOGIN
        // ------------------------------------------------------------

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true &&
                User.IsInRole("Administrator"))
            {
                return RedirectToAction(nameof(Dashboard));
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            var configuredUsername =
                _configuration["AdminSettings:Username"];

            var configuredPassword =
                _configuration["AdminSettings:Password"];

            if (string.Equals(
                    username,
                    configuredUsername,
                    StringComparison.Ordinal) &&
                string.Equals(
                    password,
                    configuredPassword,
                    StringComparison.Ordinal))
            {
                var claims = new List<Claim>
                {
                    new Claim(
                        ClaimTypes.Name,
                        username),

                    new Claim(
                        ClaimTypes.Role,
                        "Administrator")
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                return RedirectToAction(nameof(Dashboard));
            }

            // Don't reveal whether the username or password was incorrect.
            ViewBag.Error = "Invalid username or password.";

            return View();
        }

        // ------------------------------------------------------------
        // ADMIN DASHBOARD
        // ------------------------------------------------------------

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.ContactCount =
                await _context.ContactMessages.CountAsync();

            ViewBag.QuoteCount =
                await _context.QuoteRequests.CountAsync();

            ViewBag.NewContactCount =
                await _context.ContactMessages
                    .CountAsync(x => x.Status == "New");

            ViewBag.NewQuoteCount =
                await _context.QuoteRequests
                    .CountAsync(x => x.Status == "New");

            // Get ALL contact messages
            var contacts =
                await _context.ContactMessages
                    .OrderByDescending(x => x.DateSent)
                    .ToListAsync();

            // Get ALL quote requests
            var quotes =
                await _context.QuoteRequests
                    .OrderByDescending(x => x.DateSubmitted)
                    .ToListAsync();

            ViewBag.Contacts = contacts;
            ViewBag.Quotes = quotes;

            return View();
        }

        // ------------------------------------------------------------
        // CONTACT STATUS
        // ------------------------------------------------------------

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContactStatus(
    int id,
    string status)
        {
            var allowedStatuses = new[]
            {
        "New",
        "Read",
        "Resolved"
    };

            if (!allowedStatuses.Contains(status))
            {
                return BadRequest("Invalid status.");
            }

            var message =
                await _context.ContactMessages
                    .FindAsync(id);

            if (message != null)
            {
                message.Status = status;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Dashboard));
        }

        // ------------------------------------------------------------
        // QUOTE STATUS
        // ------------------------------------------------------------

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuoteStatus(
    int id,
    string status)
        {
            var allowedStatuses = new[]
            {
        "New",
        "Read",
        "Quoted",
        "Closed"
    };

            if (!allowedStatuses.Contains(status))
            {
                return BadRequest("Invalid status.");
            }

            var quote =
                await _context.QuoteRequests
                    .FindAsync(id);

            if (quote != null)
            {
                quote.Status = status;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Dashboard));
        }


        // ------------------------------------------------------------
        // LOGOUT
        // ------------------------------------------------------------

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        // ------------------------------------------------------------
        // QUOTE REQUESTS
        // ------------------------------------------------------------

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> QuoteRequests()
        {
            var quoteRequests =
                await _context.QuoteRequests
                    .OrderByDescending(x => x.DateSubmitted)
                    .ToListAsync();

            return View(quoteRequests);
        }

        // ------------------------------------------------------------
        // CONTACT MESSAGES
        // ------------------------------------------------------------

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> ContactMessages()
        {
            var contactMessages =
                await _context.ContactMessages
                    .OrderByDescending(x => x.DateSent)
                    .ToListAsync();

            return View(contactMessages);
        }

        // ------------------------------------------------------------
        // ACCESS DENIED
        // ------------------------------------------------------------

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}