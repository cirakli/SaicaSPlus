using Microsoft.EntityFrameworkCore;
using SaicaSplus.Models;


public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; }
    public DbSet<Ekran> Ekranlar { get; set; }
    public DbSet<Yetki> Yetkiler { get; set; }
}


