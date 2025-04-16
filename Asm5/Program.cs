using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ASM5.Data;
using ASM5.Models;
using ASMC4.MVC.Controllers;
var builder = WebApplication.CreateBuilder(args);

// Cấu hình DbContext cho cả Identity và API
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddHttpClient<ProductsController>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5025/"); // Địa chỉ API
});
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // Để hỗ trợ API Controller
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("APIClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5025/"); // URL gốc của API
});

var app = builder.Build();
app.UseSession();
// Áp dụng migration nếu cần
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate(); // Đảm bảo DB cập nhật
}

app.UseStaticFiles();  // <-- Thêm dòng này
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");
app.Run();
