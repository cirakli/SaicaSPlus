using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SaicaSplus.Models;

namespace SaicaSplus.Controllers
{
    public class yetki_yokController : Controller
    {
        private readonly string _connectionString;
        private readonly UserRepository _userRepository;

        public yetki_yokController(IConfiguration configuration, UserRepository userRepository)
        {
            _connectionString = configuration.GetConnectionString("SplusDb");
            _userRepository = userRepository; // UserRepository'yi al
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
    }
}
