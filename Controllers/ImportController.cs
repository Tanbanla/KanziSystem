using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using PRJ_WAREHOUSE_BIVN.Models;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Reflection;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{

    public class ImportController : Controller
    {
        public IActionResult Import_material()
        {
            return View();
        }
        public IActionResult Export_material()
        {
            return View();
        }
        public IActionResult Re_enter()
        {
            return View();
        }
        public IActionResult Log()
        {
            return View();
        }
        public IActionResult Xuatkho()
        {
            return View();
        }
        [HttpPost]
        public JsonResult _load_inv(MST_INVENTORY para)
        {
            List<MST_INVENTORY> dt = MST_INVENTORY.inventory_process(para);
            return Json(dt);
        }
        // lấy tên mã hàng hiển thị trong combobox
        public JsonResult _load_material(string group_code)
        {
            List<string> material = MST_INVENTORY._getname_material(group_code);
            return Json(material);
        }
        // lấy thông tin adid để đọc ra các mã cost, phòng ban
        public JsonResult _load_user_info(string us)
        {
            //string us = User.Identity?.Name?.Contains("\\") == true ? User.Identity.Name.Split('\\')[1] : (User.Identity?.Name ?? "Unknown");

            //string us = "loandt";
            //string us = User.FindFirst("UserId")?.Value ?? "User";
            List<User_Info> lst_cost = User_Info._info_adid(us);
            return Json(lst_cost);
        }
        public JsonResult _load_dept(string dept, string us)
        {
            //string us = "loandt";
            //string us = User.FindFirst("UserId")?.Value ?? "User";
            string cost = dept.Split(':')[0];
            string ss = dept.Split(':')[1];
            List<User_Info> lst_cost = User_Info._info_adid(us);
            var sec = lst_cost.Where(x => x.Name == ss && x.Cost_Center == cost).First();
            return Json(sec);
        }
        public JsonResult _info_material(PARAS para)
        {
            List<PARAS> mst = MATERIA.material_process(para);
            return Json(mst);
        }
        public JsonResult _load_userinventory(string group_code)
        {
            List<PE_USERNAME> _ifor = REQUEST_PROCESS._load_userinventory(group_code, "");
            return Json(_ifor);

        }
        public JsonResult _getid_user(string id)
        {
            List<PE_USERNAME> _ifor = REQUEST_PROCESS._load_userinventory("", id);
            return Json(_ifor);
        }
        public JsonResult _get_log(string madon, string ngay_tu, string ngay_den, string kho, string manguyenlieu, string loai, string phong)
        {
            List<KHO_NHAPXUAT> lst = KHO_NHAPXUAT._logg(madon, ngay_tu, ngay_den, kho, manguyenlieu, loai, phong);
            return Json(lst);
        }
        private readonly IWebHostEnvironment _env;
        public ImportController(IWebHostEnvironment env) // Inject Environment
        {
            _env = env;
        }
        [HttpPost("download_log")]
        public IActionResult download_log([FromForm] string date_to, [FromForm] string date_from)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // 1. Sửa đường dẫn Path.Combine
            string filePath = Path.Combine(_env.ContentRootPath, "Data", "Log_WH.xlsx");
            if (!System.IO.File.Exists(filePath)) return NotFound("Template file not found");

            // Dùng MemoryStream để không làm hỏng file gốc và tránh tranh chấp file (file in use)
            using (var stream = new MemoryStream())
            {
                using (var fi = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var excelPackage = new ExcelPackage(fi))
                    {
                        var excelWorksheets = excelPackage.Workbook.Worksheets.First();

                        // Clear dữ liệu cũ (Tối ưu: Chỉ clear vùng có dữ liệu)
                        int rowcount = excelWorksheets.Dimension?.End.Row ?? 1;
                        if (rowcount >= 2)
                        {
                            excelWorksheets.Cells[2, 1, rowcount, 12].Value = "";
                        }

                        List<KHO_NHAPXUAT> lst = KHO_NHAPXUAT._logg("", date_to, date_from, "", "", "", "");

                        int i = 2;
                        foreach (var item in lst)
                        {
                            excelWorksheets.Cells[i, 1].Value = i - 1;
                            excelWorksheets.Cells[i, 2].Value = item.MaNguyenLieu;
                            excelWorksheets.Cells[i, 3].Value = item.Hanhdong;
                            excelWorksheets.Cells[i, 4].Value = item.Soluong;
                            excelWorksheets.Cells[i, 5].Value = item.Loai;
                            excelWorksheets.Cells[i, 6].Value = item.Ngaynhaokho;
                            excelWorksheets.Cells[i, 7].Value = item.Nguoicapnhat;
                            excelWorksheets.Cells[i, 8].Value = item.Kho;
                            excelWorksheets.Cells[i, 9].Value = item.Khoi;
                            excelWorksheets.Cells[i, 10].Value = item.Vitri;
                            excelWorksheets.Cells[i, 11].Value = item.Phong;
                            i++;
                        }
                        excelPackage.SaveAs(stream);
                    }
                }

                string fileName = "Log_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
        public JsonResult chitiet_xuatkho()
        {
            List<CHITIET_XUATKHO> ctxk = REQUEST_PROCESS.ct_xk();
            return Json(ctxk);
        }
        public JsonResult _tonkhotheonhamay(string mahang)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var Kho = _db.GET_DATA_FROM_SQL("  select Kho, Hientai from Kho where MaNguyenLieu = '" + mahang + "' and Hientai > 0");
            List<string> lst_kho = new List<string>();
            for (int a = 0; a < Kho.Rows.Count; a++)
            {
                lst_kho.Add(Kho.Rows[a]["Kho"].ToString()!);
            }
            string soluong = Kho.Rows[0]["Hientai"].ToString()!;
            return Json(new { lst_kho, soluong });
        }
        public JsonResult _chonnhamay(string mahang, string kho)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var Kho = _db.GET_DATA_FROM_SQL("  select Kho, Hientai from Kho where MaNguyenLieu = '" + mahang + "' and Hientai > 0 and Kho = '" + kho + "'");
            string soluong = Kho.Rows[0]["Hientai"].ToString()!;
            return Json(soluong);
        }
        public JsonResult _xuatkhothucte(string code_request, string adid_nx, string manguyenlieu, string soluong, string donvi, string kho, string nguoinhan, string request, string khoi, string phong, string vitri)
        {
            var check = REQUEST_PROCESS._xuatkho(code_request, adid_nx, manguyenlieu, soluong, donvi, kho, nguoinhan, khoi, phong, vitri, request);
            return Json(check);
        }
    }
}
