using Microsoft.AspNetCore.Mvc;
using SaicaSplus.Services; // YetkiServisi sınıfını eklemek için gerekli
using System.Threading.Tasks;

namespace SaicaSplus.Controllers
{
    public class EkranController : Controller
    {
        private readonly YetkiServisi _yetkiServisi;

        public EkranController(YetkiServisi yetkiServisi)
        {
            _yetkiServisi = yetkiServisi;
        }

        public async Task<IActionResult> Ekle()
        {
            int userId = 1; // Burada oturum açan kullanıcının ID'si yer alacak
            int ekranId = 1; // Örnek ekran id
            int islemId = 2; // Örnek işlem id (ekleme işlemi)

            // Kullanıcının yetkili olup olmadığını kontrol et
            bool yetkili = await _yetkiServisi.KullaniciYetkiliMi(userId, ekranId, islemId);

            if (yetkili)
            {
                // Eğer yetkiliyse, sayfayı döndür
                return View();
            }
            else
            {
                // Eğer yetkili değilse, yetkisiz sayfasına yönlendir
                return RedirectToAction("Yetkisiz", "Hata");
            }
        }
    }
}
