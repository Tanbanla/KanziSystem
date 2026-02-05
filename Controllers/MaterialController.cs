using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Implementations;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Material;
using static PRJ_WAREHOUSE_BIVN.View_Models.Material.MaterialVM;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class MaterialController : BaseAuthController
    {
        private readonly IBaoGiaConfirmNameService _confirmNameService;
        private readonly INhomViTriService _nhomViTriService;
        private readonly IBaoGiaService _baoGiaService;
        public MaterialController(IBaoGiaConfirmNameService confirmNameService, INhomViTriService nhomViTriService, IBaoGiaService baoGiaService)
        {
            _confirmNameService = confirmNameService;
            _nhomViTriService = nhomViTriService;
            _baoGiaService = baoGiaService;
        }
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
        public async Task<IActionResult> ConfirmName()
        {
            // Determine role from query or default to UserPUR
            var role = (Request.Query["role"].ToString() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(role)) role = "UserPUR"; // UserShip | UserAcc | UserPUR
            ViewBag.Role = role;
            var vitris = await LoadNhomViTriDataAsync();
            var vm = new MaterialVM
            {
                vitris = vitris
            };
            return View(vm);
        }
        private async Task<List<ACC_NHOMVITRIDTO>> LoadNhomViTriDataAsync()
        {
            var nhomViTri = await _nhomViTriService.GetAllNhomViTriAsync();
            return nhomViTri.Data ?? new List<ACC_NHOMVITRIDTO>();
        }
        // Search confirm list
        [HttpPost]
        public async Task<IActionResult> SearchConfirmName([FromBody] ConfirmNameSearchRequest req)
        {
            var result = await _confirmNameService.SearchAsync(req.TenHang, req.SoDon, req.TrangThai, req.Section, req.pageIndex, req.pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        // Save inline changes by role
        [HttpPost]
        public async Task<IActionResult> SaveConfirmName([FromBody] ConfirmNameSaveRequest req)
        {
            var result = await _confirmNameService.SaveConfirmNameAsync(req.Id, req.TenHaiQuan, req.MaHangNoiBo, req.Role, GetCurrentUserId());
            if(!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Success);
        }

        // Approve (agree) and update base request
        [HttpPost]
        public async Task<IActionResult> ApproveConfirmName([FromBody] ConfirmNameActionRequest req)
        {
            var result = await _confirmNameService.ApproveConfirmNameAsync(req.Id, GetCurrentUserId());
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Success);
        }

        // Reject with reason
        [HttpPost]
        public async Task<IActionResult> RejectConfirmName([FromBody] ConfirmNameRejectRequest req)
        {
            var result = await _confirmNameService.RejectConfirmNameAsync(req.Id, req.LyDo ,GetCurrentUserId());
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Success);
        }
        // tìm kiếm theo ID đơn báo giá 
        [HttpPost]
        public async Task<IActionResult> SearchID([FromBody] int id)
        {
            var result = await _baoGiaService.GetByIdAsync(id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
    }

}
