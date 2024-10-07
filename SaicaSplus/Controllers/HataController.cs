using Microsoft.AspNetCore.Mvc;

namespace SaicaSplus.Controllers
{
    public class HataController : Controller
    {
        // Yetkisiz erişim sayfası
        public IActionResult Yetkisiz()
        {
            return View(); // Yetkisiz sayfasını göster
        }
    }
}
