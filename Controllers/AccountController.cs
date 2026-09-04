using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using System.Security.Claims;
using PRJ_WAREHOUSE_BIVN.View_Models.Login;
using Microsoft.AspNetCore.Localization;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class AccountController : Controller
    {
        private readonly ITmUserService _userService;
        private readonly IConfiguration _configuration;

        public AccountController(ITmUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            try
            {
                var loginResult = await _userService.Login(model.Username, model.Password);
                
                if (loginResult.Success && loginResult.Data != null)
                {
                    // lấy role
                    var roleAsync = await _userService.GetRoleAsync(model.Username);

                    // Tạo claims cho user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginResult.Data.FULLNAME ?? loginResult.Data.CHR_USERID),
                        new Claim(ClaimTypes.NameIdentifier, loginResult.Data.ID.ToString()),
                        new Claim("UserId", loginResult.Data.CHR_USERID),
                        new Claim("EmployeeId", loginResult.Data.CHR_EMPLOYEE_ID ?? ""),
                        new Claim("Section", loginResult.Data.CHR_SECTION ?? ""),
                        new Claim("Email", loginResult.Data.dia_chi_mail ?? ""),
                        new Claim("Permission", loginResult.Data.phan_quyen.ToString()),
                        new Claim("Department", loginResult.Data.phong_ban ?? ""),
                        new Claim("Roles", roleAsync.Data ?? "")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3),
                        IsPersistent = model.RememberMe,
                        AllowRefresh = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Lưu thông tin session bổ sung
                    HttpContext.Session.SetString("FullName", loginResult.Data.FULLNAME ?? loginResult.Data.CHR_USERID);
                    HttpContext.Session.SetString("UserId", loginResult.Data.CHR_USERID);
                    HttpContext.Session.SetInt32("UserIdInt", loginResult.Data.ID);
                    HttpContext.Session.SetString("LoginTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    // Đánh dấu phiên làm việc đã khởi tạo để middleware kiểm tra
                    HttpContext.Session.SetString("SessionActive", "true");

                    // Redirect đến trang được yêu cầu hoặc trang chủ
                    var returnUrl = model.ReturnUrl;
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", loginResult.Message ?? "Tên đăng nhập không chính xác.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã có lỗi xảy ra khi đăng nhập. Vui lòng thử lại :"+ ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Xóa session
            HttpContext.Session.Clear();
            
            // đăng xuất authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Profile()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var fullName = User.FindFirst(ClaimTypes.Name)?.Value;
            var loginTime = HttpContext.Session.GetString("LoginTime");
            
            ViewBag.UserId = userId;
            ViewBag.FullName = fullName;
            ViewBag.LoginTime = loginTime;
            
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult SetCulture(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            return LocalRedirect(returnUrl ?? $"{HttpContext.Request.PathBase}/Account/Login");
        }
    }
}
