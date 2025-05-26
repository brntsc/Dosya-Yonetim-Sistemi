using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using uyg.UI.Models;

namespace uyg.UI.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;

        public AdminController(ILogger<AdminController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _apiBaseUrl = _configuration["ApiBaseURL"];
        }

        public IActionResult Login()
        {
            ViewBag.ApiBaseURL = _apiBaseUrl;
            return View();
        }

        public IActionResult Dashboard()
        {
            ViewBag.ApiBaseURL = _apiBaseUrl;
            return View();
        }

        public IActionResult Categories()
        {
            ViewBag.ApiBaseURL = _apiBaseUrl;
            return View();
        }

        public IActionResult Files()
        {
            ViewBag.ApiBaseURL = _apiBaseUrl;
            return View();
        }

        public IActionResult FileTag()
        {
            ViewBag.ApiBaseURL = _apiBaseUrl;
            return View();
        }

        public IActionResult Logout()
        {
            // Local storage'daki token'ı temizle
            Response.Cookies.Delete("token");
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 