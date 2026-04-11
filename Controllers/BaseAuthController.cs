using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.Filters;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    [Authorize]
    [SessionTimeout] // Ki?m tra session timeout
    public class BaseAuthController : Controller
    {
        protected string GetCurrentUserId()
        {
            return User.FindFirst("UserId")?.Value ?? "";
        }

        protected int GetCurrentUserIdInt()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        protected string GetCurrentUserFullName()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";
        }

        protected string GetCurrentUserSection()
        {
            return User.FindFirst("Section")?.Value ?? "";
        }

        protected string GetCurrentUserDepartment()
        {
            return User.FindFirst("Department")?.Value ?? "";
        }
        protected string GetRolesUser()
        {
            return User.FindFirst("Roles")?.Value ?? "";
        }

        protected int GetCurrentUserPermission()
        {
            var permissionClaim = User.FindFirst("Permission")?.Value;
            return int.TryParse(permissionClaim, out int permission) ? permission : 0;
        }
    }
}
