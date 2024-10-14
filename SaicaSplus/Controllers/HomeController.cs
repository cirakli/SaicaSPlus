using Microsoft.AspNetCore.Mvc;
using SaicaSplus.Models;
using SaicaSplus.Services;
using System.Diagnostics;

namespace SaicaSplus.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserRepository _userRepository;

        public HomeController(ILogger<HomeController> logger, UserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            // Kullanıcı adını oturumdan al
            var username = HttpContext.Session.GetString("Username");

            if (username != null)
            {
                // Kullanıcı bilgilerini al
                var userDetails = _userRepository.GetUserDetails(username);
                ViewBag.FirstName = userDetails.FirstName;
                ViewBag.LastName = userDetails.LastName;
            }

            return View();
        }

        public IActionResult Login()
        {
            return View();
        }


        public IActionResult Yönetim()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (_userRepository.HasPermission(username, "yönetim"))
            {
                return View();
            }
            else
            {
                ViewBag.ErrorMessage = "Yetkiniz bulunmamaktadır.";
                return RedirectToAction("yetki_yok");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult yetki_yok()
        {
            return View(); // yetki_yok.cshtml dosyasını döndürür
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
