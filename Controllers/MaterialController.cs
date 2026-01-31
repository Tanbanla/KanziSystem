using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class MaterialController : Controller
    {
        // MARK: Confirm Name actions use EF context directly
        public IActionResult Material()
        {
            return View();
        }
        public IActionResult Creat_Material()
        {
            return View();
        }
        [HttpPost]
        public JsonResult load_material (PARAS para)
        {
            List<PARAS> dt = MATERIA.material_process(para);
            return Json(dt);
        }
        // MARk: Màn hình xác nhận tên
        public IActionResult ConfirmName()
        {
            // Determine role from query or default to UserPUR
            var role = (Request.Query["role"].ToString() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(role)) role = "UserPUR"; // UserShip | UserAcc | UserPUR
            ViewBag.Role = role;
            return View();
        }

        // Search confirm list
        [HttpPost]
        public async Task<IActionResult> SearchConfirmName([FromBody] ConfirmNameSearchRequest req)
        {

            return Ok(new { });
        }

        // Save inline changes by role
        [HttpPost]
        public async Task<IActionResult> SaveConfirmName([FromBody] ConfirmNameSaveRequest req)
        {
            return Ok();
        }

        // Approve (agree) and update base request
        [HttpPost]
        public async Task<IActionResult> ApproveConfirmName([FromBody] ConfirmNameActionRequest req)
        {
            return Ok();
        }

        // Reject with reason
        [HttpPost]
        public async Task<IActionResult> RejectConfirmName([FromBody] ConfirmNameRejectRequest req)
        {
            return Ok();
        }
    }

    // Request/Response DTOs for ConfirmName APIs
    public class ConfirmNameSearchRequest
    {
        public string? TenHang { get; set; }
        public string? SoDon { get; set; }
        public string? TrangThai { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ConfirmNameRow
    {
        public int Id { get; set; }
        public int IdRequestQuote { get; set; }
        public string? SoDon { get; set; }
        public string? TenHaiQuan { get; set; }
        public string? MaHangNoiBo { get; set; }
        public string? TrangThai { get; set; }
        public string? CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string? UserShip { get; set; }
        public DateTime? DtmUserShip { get; set; }
        public string? UserAcc { get; set; }
        public DateTime? DtmUserAcc { get; set; }
        public string? UserPur { get; set; }
        public DateTime? DtmUserPur { get; set; }
        public string? Note { get; set; }
        public string? LyDo { get; set; }
    }

    public class ConfirmNameSaveRequest
    {
        public int Id { get; set; }
        public string? TenHaiQuan { get; set; }
        public string? MaHangNoiBo { get; set; }
        public string? Role { get; set; }
    }

    public class ConfirmNameActionRequest
    {
        public int Id { get; set; }
    }

    public class ConfirmNameRejectRequest
    {
        public int Id { get; set; }
        public string? LyDo { get; set; }
    }
}
