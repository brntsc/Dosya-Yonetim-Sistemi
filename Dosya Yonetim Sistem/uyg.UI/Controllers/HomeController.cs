using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using uyg.UI.Models;

namespace uyg.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewBag.ApiBaseURL = _configuration["ApiBaseURL"];
            return View();
        }

        public IActionResult Files(int? catId)
        {
            ViewBag.ApiBaseURL = _configuration["ApiBaseURL"];
            ViewBag.CatId = catId ?? 0;
            return View();
        }

        public IActionResult Categories()
        {
            ViewBag.ApiBaseURL = _configuration["ApiBaseURL"];
            return View();
        }

        public IActionResult FileTag()
        {
            ViewBag.ApiBaseURL = _configuration["ApiBaseURL"];
            return View();
        }

        public IActionResult Profile()
        {
            ViewBag.ApiBaseURL = _configuration["ApiBaseURL"];
            return View();
        }

        public IActionResult Login()
        {
            ViewBag.ApiBaseURL = _configuration["ApiBaseURL"];
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
