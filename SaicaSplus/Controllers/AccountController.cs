using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    private readonly UserRepository _userRepository;
    private readonly ActiveDirectoryService _activeDirectoryService;

    public AccountController(UserRepository userRepository, ActiveDirectoryService activeDirectoryService)
    {
        _userRepository = userRepository; // UserRepository'yi burada kullanın
        _activeDirectoryService = activeDirectoryService;

    }

    public IActionResult Login()
    {
        return View();
    }




    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        // Kullanıcının Active Directory'de doğrulanmasını kontrol et
        bool isValidUser = _activeDirectoryService.ValidateUser("gsaica", username, password);

        if (isValidUser)
        {
            // SQL veritabanında kullanıcının var olup olmadığını kontrol et
            var userDomains = _userRepository.GetUserDomains();

            // Kullanıcı adı SQL veritabanında mevcut mu?
            if (userDomains.Contains(username))
            {
                // Giriş başarılı
                HttpContext.Session.SetString("Username", username); // Kullanıcı adını oturumda tut
                TempData["SuccessMessage"] = "Giriş başarılı!";
                return RedirectToAction("Index", "Home"); // Ana sayfaya yönlendir
            }
            else
            {
                // Kullanıcı SQL veritabanında mevcut değil
                ViewBag.ErrorMessage = "Kullanıcının Splus Giriş Yetkisi Bulunmamaktadır.";
                return View(); // Aynı sayfayı tekrar göster
            }
        }
        else
        {
            // Giriş başarısız
            ViewBag.ErrorMessage = "Kullanıcı adı veya şifre yanlış.";
            return View(); // Aynı sayfayı tekrar göster
        }
    }

    public ActionResult Logout()
    {
        // Perform logout logic (e.g., clearing session, authentication cookie)
        // Redirect to the login view after logging out
        return RedirectToAction("Login", "Account");
    }

   

}
