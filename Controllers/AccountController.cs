using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Services.Service.Implementations;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Login;
using System.Security.Claims;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class AccountController : Controller
    {
        private readonly ITmUserService _userService;
        private readonly IBaoGiaWorkflowRoleService _baoGiaWorkflowRoleService;
        private readonly IEmployeeWorkingService _employeeWorkingService;
        private readonly IConfiguration _configuration;

        public AccountController(ITmUserService userService, IBaoGiaWorkflowRoleService baoGiaWorkflowRoleService, IEmployeeWorkingService employeeWorkingService, IConfiguration configuration)
        {
            _userService = userService;
            _baoGiaWorkflowRoleService = baoGiaWorkflowRoleService;
            _employeeWorkingService = employeeWorkingService;
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
                ModelState.AddModelError("", "Đã có lỗi xảy ra khi đăng nhập. Vui lòng thử lại :" + ex.Message);
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


        // MARK: màn hình quản lý user
        public async Task<IActionResult> UserManagement()
        {
            var role = User.FindFirst("Roles")?.Value;
            if (role != "PUR")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var roles = await _baoGiaWorkflowRoleService.GetAllAsync();

            var vm = new UserManagerModel
            {
                Roles = (List<BaoGia_WorkflowRoleDTO>)(roles.Data ?? new List<BaoGia_WorkflowRoleDTO>())
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SearchUser([FromBody] UserSearchModel searchModel)
        {
            if (searchModel == null)
            {
                return BadRequest("Invalid search parameters.");
            }
            var result = await _userService.SearchUserAsync(searchModel);
            if (result.Success)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result.Message ?? "Error occurred while searching for users.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] UserInsertModel userInsert)
        {
            if (userInsert == null)
            {
                return BadRequest("Invalid user data.");
            }
            var result = await _userService.RegisterUserAsync(userInsert);
            if (result.Success)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result.Message ?? "Error occurred while registering the user.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromBody] UserInsertModel userUpdate)
        {
            if (userUpdate == null)
            {
                return BadRequest("Invalid user data.");
            }
            var result = await _userService.UpdateUserAsync(userUpdate);
            if (result.Success)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result.Message ?? "Error occurred while updating the user.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid user ID.");
            }
            var result = await _userService.DeleteUserAsync(userId);
            if (result.Success)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result.Message ?? "Error occurred while deleting the user.");
            }
        }
        // Lấy thông tin nhân viên theo ADID or MNV
        [HttpGet]
        public async Task<JsonResult> GetEmployeeWorkingByIdAsync(string adidOrMnv)
        {
            var resp = await _employeeWorkingService.GetEmployeeWorkingByIdAsync(adidOrMnv);
            if (resp == null || !resp.Success)
            {
                return Json(new { success = false, message = resp?.Message ?? "Error" });
            }
            var data = resp.Data ?? new List<dynamic>();
            return Json(new { success = true, data });
        }
    }
}
