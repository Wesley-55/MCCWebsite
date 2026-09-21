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

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(Dashboard));
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string username,
            string password)
        {
            var configuredUsername =
                _configuration["AdminSettings:Username"];

            var configuredPassword =
                _configuration["AdminSettings:Password"];

            if (username == configuredUsername &&
                password == configuredPassword)
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

                var identity =
                    new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                var principal =
                    new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                return RedirectToAction(nameof(Dashboard));
            }

            ViewBag.Error =
                "Invalid username or password.";

            return View();
        }


        [Authorize(Roles = "Administrator")]
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

            var contacts =
                await _context.ContactMessages
                    .OrderByDescending(x => x.DateSent)
                    .Take(10)
                    .ToListAsync();

            var quotes =
                await _context.QuoteRequests
                    .OrderByDescending(x => x.DateSubmitted)
                    .Take(10)
                    .ToListAsync();

            ViewBag.Contacts = contacts;
            ViewBag.Quotes = quotes;

            return View();
        }


        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContactStatus(
            int id,
            string status)
        {
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


        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuoteStatus(
            int id,
            string status)
        {
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


        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> QuoteRequests()
        {
            var quoteRequests = await _context.QuoteRequests
                .OrderByDescending(x => x.DateSubmitted)
                .ToListAsync();

            return View(quoteRequests);
        }


        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> ContactMessages()
        {
            var contactMessages = await _context.ContactMessages
                .OrderByDescending(x => x.DateSent)
                .ToListAsync();

            return View(contactMessages);
        }
    }
}