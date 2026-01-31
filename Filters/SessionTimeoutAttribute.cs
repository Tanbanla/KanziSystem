using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PRJ_WAREHOUSE_BIVN.Filters
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            
            if (user.Identity?.IsAuthenticated == true)
            {
                // Ki?m tra xem session có h?t h?n không
                var loginTime = context.HttpContext.Session.GetString("LoginTime");
                
                if (string.IsNullOrEmpty(loginTime))
                {
                    // Session ?ã h?t h?n, redirect v? login
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                    return;
                }
            }
            
            base.OnActionExecuting(context);
        }
    }
}