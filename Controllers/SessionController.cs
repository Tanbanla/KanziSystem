using Microsoft.AspNetCore.Mvc;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class SessionController : BaseAuthController
    {
        public IActionResult Info()
        {
            var sessionInfo = new
            {
                UserId = GetCurrentUserId(),
                FullName = GetCurrentUserFullName(),
                LoginTime = HttpContext.Session.GetString("LoginTime"),
                SessionId = HttpContext.Session.Id,
                ExpiryTime = DateTime.Now.AddHours(3).ToString("dd/MM/yyyy HH:mm:ss"),
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false
            };

            return Json(sessionInfo);
        }

        public IActionResult CheckTimeout()
        {
            var loginTimeStr = HttpContext.Session.GetString("LoginTime");
            
            if (string.IsNullOrEmpty(loginTimeStr))
            {
                return Json(new { expired = true, message = "Phiên đăng nhập đã hết hạn" });
            }

            if (DateTime.TryParse(loginTimeStr, out DateTime loginTime))
            {
                var timeRemaining = loginTime.AddHours(3) - DateTime.Now;
                
                return Json(new 
                { 
                    expired = timeRemaining.TotalMinutes <= 0,
                    timeRemaining = timeRemaining.TotalMinutes > 0 ? Math.Round(timeRemaining.TotalMinutes) : 0,
                    message = timeRemaining.TotalMinutes <= 0 ? "Phiên đăng nhập đã hết hạn" : $"Còn lại {Math.Round(timeRemaining.TotalMinutes)} phút"
                });
            }

            return Json(new { expired = true, message = "Không thể xác định thời gian đăng nhập" });
        }
    }
}