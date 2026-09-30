using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Extensions;
using PRJ_WAREHOUSE_BIVN.Models_Agent;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Models_Working;
using PRJ_WAREHOUSE_BIVN.Services.Configs.AutoMapper;
using System.Data;
using System.Data.SqlClient;
using System.Globalization; 

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

var applicationInstanceId = Guid.NewGuid().ToString("N");
builder.Configuration["ApplicationInstanceId"] = applicationInstanceId;

var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"]
    ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "PRJ_WAREHOUSE_BIVN",
        "DataProtection-Keys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .SetApplicationName("PRJ_WAREHOUSE_BIVN")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();


var costManagerConnection = builder.Configuration.GetConnectionString("CostManagerConnection");
var workingControlConnection = builder.Configuration.GetConnectionString("WorkingControlConnection");
var agentConnection = builder.Configuration.GetConnectionString("AgentConnection");
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
    options.IdleTimeout = TimeSpan.FromHours(10); // Session timeout 10 tiếng
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
        options.ExpireTimeSpan = TimeSpan.FromDays(3650);
        options.SlidingExpiration = false;
        options.Cookie.Name = ".PRJ_WAREHOUSE_BIVN.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Events.OnValidatePrincipal = context =>
        {
            var cookieInstanceId = context.Principal?.FindFirst("ApplicationInstanceId")?.Value;
            if (!string.Equals(cookieInstanceId, applicationInstanceId, StringComparison.Ordinal))
            {
                context.RejectPrincipal();
            }

            return Task.CompletedTask;
        };
    });

// Cấu hình DbContext với SQL Server


builder.Services.AddDbContext<COST_MANAGEMENTContext>(options =>
    options.UseSqlServer(costManagerConnection));
builder.Services.AddDbContext<WorkingSystemContext> (options =>options.UseSqlServer(workingControlConnection));
builder.Services.AddDbContext<AgentContext>(options => options.UseSqlServer(agentConnection));


builder.Services.Configure<ConnectionStringOptions>(builder.Configuration.GetSection("ConnectionStrings"));

// Initialize static EmailSender with settings and agent connection string
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
EmailSender.Initialize(emailSettings, agentConnection);

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
app.UseAuthorization();


app.MapControllerRoute(
name: "default",
pattern: "{controller=Account}/{action=Login}/{id?}"); // Đặt trang login làm trang mặc định

app.Run();
