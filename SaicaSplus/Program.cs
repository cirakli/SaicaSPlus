using Microsoft.EntityFrameworkCore;
using SaicaSplus.Services;


var builder = WebApplication.CreateBuilder(args);


// User Secrets'ten bağlantı dizesini al
var connectionString = builder.Configuration.GetConnectionString("SplusDb");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<UserRepository>(); // UserRepository servisini ekle
builder.Services.AddScoped<ActiveDirectoryService>(); // ActiveDirectoryService servisini ekle
builder.Services.AddScoped<YetkiServisi>(); // YetkiServisi DI olarak ekleniyor

builder.Services.AddSession(); // Oturum yönetimini ekle


//// 2. Veritabanı bağlantı ayarını yükleme
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("SplusDb")));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.UseSession(); // Oturum yönetimini kullan

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
