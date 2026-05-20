using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Models_Working;
using PRJ_WAREHOUSE_BIVN.Services.Configs.AutoMapper;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using PRJ_WAREHOUSE_BIVN.Extensions;
using Microsoft.AspNetCore.Authentication;
using PRJ_WAREHOUSE_BIVN.Models_Agent;
using System.Globalization; 

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();


var costManagerConnection = builder.Configuration.GetConnectionString("CostManagerConnection");
var workingControlConnection = builder.Configuration.GetConnectionString("WorkingControlConnection");
var agentConnection = builder.Configuration.GetConnectionString("AgentConnection");
var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "";
// Add services to the container and require authentication globally by default.

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// Cấu hình Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(10); // Session timeout 3 tiếng
    options.Cookie.HttpOnly = true; // Bảo mật cookie
    options.Cookie.IsEssential = true; // Cookie cần thiết
    options.Cookie.Name = ".PRJ_WAREHOUSE_BIVN.Session";
});

// Cấu hình Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn đến trang login
        options.LogoutPath = "/Account/Logout"; // Đường dẫn logout
        options.AccessDeniedPath = "/Account/AccessDenied"; // Đường dẫn khi bị từ chối truy cập
        options.ExpireTimeSpan = TimeSpan.FromHours(10); // Thời gian hết hạn cookie 3 tiếng
        options.SlidingExpiration = true; // Gia hạn cookie khi user hoạt động
        options.Cookie.Name = ".PRJ_WAREHOUSE_BIVN.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Cấu hình DbContext với SQL Server


builder.Services.AddDbContext<COST_MANAGEMENTContext>(options =>
    options.UseSqlServer(costManagerConnection));
builder.Services.AddDbContext<WorkingSystemContext> (options =>options.UseSqlServer(workingControlConnection));
builder.Services.AddDbContext<AgentContext>(options => options.UseSqlServer(agentConnection));


builder.Services.Configure<ConnectionStringOptions>(builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddTransient<IDbConnection>(sp => new SqlConnection(costManagerConnection));
builder.Services.AddTransient<IDbConnection>(sp => new SqlConnection(workingControlConnection));
builder.Services.AddTransient<IDbConnection>(sp => new SqlConnection(agentConnection));

// khai bao services
builder.Services.AddAppServices();

// Khai báo AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Index");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Localization middleware
var supportedCultures = new[] { "vi", "en", "ja" }; // Tạo một Culture chung dựa trên tiếng Việt nhưng ép dấu chấm làm thập phân
var viCulture = new CultureInfo("vi-VN");
viCulture.NumberFormat.NumberDecimalSeparator = ".";
viCulture.NumberFormat.CurrencyDecimalSeparator = ".";
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("vi")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures); // Quan trọng: Ghi đè quy tắc định dạng số cho tất cả Culture được hỗ trợ
     foreach (var culture in localizationOptions.SupportedCultures) {     culture.NumberFormat.NumberDecimalSeparator = ".";     culture.NumberFormat.CurrencyDecimalSeparator = "."; } app.UseRequestLocalization(localizationOptions);

// Thêm middleware cho session và authentication
app.UseSession();
app.UseAuthentication(); // Phải đặt trước UseAuthorization
// Tự động xóa cookie khi hết phiên đăng nhập (session hết hạn)
app.Use(async (context, next) =>
{
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        // Kiểm tra session còn tồn tại không
        var hasSession = !string.IsNullOrEmpty(context.Session.GetString("UserId"));
        if (!hasSession)
        {
            // Xóa thông tin đăng nhập (cookie auth) khi session hết hạn
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Đưa người dùng về trang đăng nhập
            var loginUrl = $"{baseUrl}/Account/Login";
            context.Response.Redirect(loginUrl);
            return;
        }
    }
    await next();
});
app.UseAuthorization();


app.MapControllerRoute(
name: "default",
pattern: "{controller=Account}/{action=Login}/{id?}"); // Đặt trang login làm trang mặc định

app.Run();
