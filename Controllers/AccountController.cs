using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using System.Security.Claims;
using PRJ_WAREHOUSE_BIVN.View_Models.Login;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class AccountController : Controller
    {
        private readonly ITmUserService _userService;

        public AccountController(ITmUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            // N?u ?ã ??ng nh?p r?i thì redirect v? trang ch?
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var loginResult = await _userService.Login(model.Username, model.Password);
                
                if (loginResult.Success && loginResult.Data != null)
                {
                    // T?o claims cho user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginResult.Data.FULLNAME ?? loginResult.Data.CHR_USERID),
                        new Claim(ClaimTypes.NameIdentifier, loginResult.Data.ID.ToString()),
                        new Claim("UserId", loginResult.Data.CHR_USERID),
                        new Claim("EmployeeId", loginResult.Data.CHR_EMPLOYEE_ID ?? ""),
                        new Claim("Section", loginResult.Data.CHR_SECTION ?? ""),
                        new Claim("Email", loginResult.Data.dia_chi_mail ?? ""),
                        new Claim("Permission", loginResult.Data.phan_quyen.ToString()),
                        new Claim("Department", loginResult.Data.phong_ban ?? "")
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

                    // L?u thông tin session b? sung
                    HttpContext.Session.SetString("FullName", loginResult.Data.FULLNAME ?? loginResult.Data.CHR_USERID);
                    HttpContext.Session.SetString("UserId", loginResult.Data.CHR_USERID);
                    HttpContext.Session.SetInt32("UserIdInt", loginResult.Data.ID);
                    HttpContext.Session.SetString("LoginTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                    // Redirect ??n trang ???c yêu c?u ho?c trang ch?
                    var returnUrl = Request.Query["returnUrl"].FirstOrDefault();
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
                ModelState.AddModelError("", "Đã có lỗi xảy ra khi đăng nhập. Vui lòng thử lại");
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
            
            // ??ng xu?t authentication
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
    }
}