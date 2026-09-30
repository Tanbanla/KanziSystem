using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PRJ_WAREHOUSE_BIVN.Filters
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Trạng thái đăng nhập được lưu trong authentication cookie.
            // Session có thể bị mất khi IIS recycle nên không dùng session để logout user.
            base.OnActionExecuting(context);
        }
    }
}
