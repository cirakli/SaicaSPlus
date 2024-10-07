using Microsoft.EntityFrameworkCore; // For AnyAsync

namespace SaicaSplus.Services


{
    public class YetkiServisi
    {
        private readonly ApplicationDbContext _context;

        public YetkiServisi(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kullanıcıya yetki kontrolü (sayfa ve işlem bazında)
        public async Task<bool> KullaniciYetkiliMi(int userId, int ekranId, int islemId)
        {
            return await _context.Yetkiler
                .AnyAsync(y => y.SUserId == userId && y.EkranId == ekranId && y.IslemId == islemId);
        }
    }
}
