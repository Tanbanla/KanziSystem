using DocumentFormat.OpenXml.Office2021.Drawing.SketchyShapes;
using DocumentFormat.OpenXml.Spreadsheet;
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
        public IActionResult Tainhap()
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
        public JsonResult _get_log(string madon, string ngay_tu, string ngay_den, string kho, string manguyenlieu, string loai,  string us)
        {
            List<KHO_NHAPXUAT> lst = KHO_NHAPXUAT._logg(madon, ngay_tu, ngay_den, kho, manguyenlieu, loai,  us);
            return Json(lst);
        }
        private readonly IWebHostEnvironment _env;
        public ImportController(IWebHostEnvironment env) // Inject Environment
        {
            _env = env;
        }
        [HttpGet]
        public ActionResult ExportToExcel_Log()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string path = Path.Combine(_env.ContentRootPath, "Data");
            string templatePath = Path.Combine(path, "File_export.xlsx");
            string tempFileName = $"Temp_{Guid.NewGuid()}.xlsx";
            string tempPath = Path.Combine(path, tempFileName);

            try
            {
                System.IO.File.Copy(templatePath, tempPath, true);
                FileInfo fileInfo = new FileInfo(tempPath);
                using (var package = new ExcelPackage(fileInfo))
                {
                    var wsMaterial = package.Workbook.Worksheets[1];
                    List<string> DataListMaterial = MST_INVENTORY._getname_material();
                    //for (int idx = 0; idx < DataListMaterial.Count; idx++)
                    //{
                    //    wsMaterial.Cells["A" + (idx + 2)].Value = DataListMaterial[idx];
                    //}

                    //var wsDept = package.Workbook.Worksheets[2];
                    //List<string> DataForDept = User_Info._GetCostCenter();
                    //for (int idx = 0; idx < DataForDept.Count; idx++)
                    //{
                    //    wsDept.Cells["A" + (idx + 2)].Value = DataForDept[idx];
                    //}

                    //var wsLocation = package.Workbook.Worksheets[3];
                    //List<string> DataLocation = User_Info._GetLocation();
                    //for (int idx = 0; idx < DataLocation.Count; idx++)
                    //{
                    //    wsLocation.Cells["A" + (idx + 2)].Value = DataLocation[idx];
                    //}

                    package.Save();
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(tempPath);
                System.IO.File.Delete(tempPath);
                return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "FormatTemplateRequest.xlsx"
            );
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi xử lý: {ex.Message}");
            }

        }
        public JsonResult chitiet_xuatkho(string mayeucau, string nguoitao)
        {
            List<CHITIET_XUATKHO> ctxk = REQUEST_PROCESS.ct_xk(mayeucau, nguoitao);
            return Json(ctxk);
        }
        public JsonResult _tonkhotheonhamay(string mahang)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var Kho = _db.GET_DATA_FROM_SQL("select Kho, Hientai from Kho where MaNguyenLieu = '" + mahang + "' and Hientai > 0");
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
        public JsonResult _xuatkhothucte(string code_request, string adid_nx, string nguoinhan, string nguoixuatkho, string thoigian, string manguyenlieu, string soluong, string giathucte, string donvi, string kho, string tongchiphi, string vitri, string phong, string khoi, string tongchiphiold, string id_rq)
        {
            if (tongchiphi == "0")
            {
                tongchiphi = tongchiphiold;
            }
            var check = REQUEST_PROCESS._xuatkho(code_request, adid_nx, nguoinhan, nguoixuatkho, thoigian, manguyenlieu, soluong, giathucte, donvi, kho, tongchiphi, vitri, phong, khoi, id_rq);
            return Json(check);
        }
        public JsonResult _load_xuatkhohang(string mayeucau, string nguoitao, string khoi)
        {
            if(khoi == "GA")
            {
                var list = Models.REQUEST_PROCESS_GA._load_tonkhoxuathang(mayeucau, nguoitao, khoi);
                return Json(list);
            }
           
            if(khoi == "PROD")
            {
                var list = Models.REQUEST_PROCESS._load_tonkhoxuathang(mayeucau, nguoitao, khoi);
                return Json(list);
            }
            return Json("Không có dữ liệu");
        }
        public JsonResult _load_body_detail(string code_request)
        {
            var list = Models.REQUEST_PROCESS._load_body_detail(code_request);
            return Json(list);
        }
        public JsonResult _load_modal_detail(string code_request)
        {
            var list = Models.REQUEST_PROCESS._load_body_detail(code_request);
            var load = Models.REQUEST_PROCESS._load_request(code_request);
            return Json(new
            {
                list = list,
                load = load
            });
        }
        public byte[] ExportToExcel_FileEx<T>(List<T> data, string sheetName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(sheetName);

                // Load dữ liệu từ List vào sheet, bắt đầu từ ô A1, tự động tạo Header
                worksheet.Cells["A1"].LoadFromCollection(data, false);

                // Format Header (Bôi đậm)
                using (var range = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                {
                    range.Style.Font.Bold = true;
                }
                // Tự động căn chỉnh độ rộng cột
                worksheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }
        [HttpGet("export")]
        public IActionResult Export()
        {
            var data = new List<KHO_NHAPXUAT> {
            new KHO_NHAPXUAT {  }
        };
            var fileContents = ExportToExcel_FileEx(data, "Students");

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "StudentList.xlsx"
            );
        }

        [HttpGet]
        public ActionResult ExportToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string pathDir = Path.Combine(_env.ContentRootPath, "Data");
            string templatePath = Path.Combine(pathDir, "File_import_ex.xlsx");
            string tempFileName = $"Temp_{Guid.NewGuid()}.xlsx";
            string tempPath = Path.Combine(pathDir, tempFileName);

            try
            {
                System.IO.File.Copy(templatePath, tempPath, true);
                FileInfo fileInfo = new FileInfo(tempPath);
                using (var package = new ExcelPackage(fileInfo))
                {
                    var wsMaterial = package.Workbook.Worksheets[1];
                    List<string> DataListMaterial = MST_INVENTORY._getname_material();
                    for (int idx = 0; idx < DataListMaterial.Count; idx++)
                    {
                        wsMaterial.Cells["A" + (idx + 2)].Value = DataListMaterial[idx];
                    }

                    var wsDept = package.Workbook.Worksheets[2];
                    List<string> DataForDept = User_Info._GetCostCenter();
                    for (int idx = 0; idx < DataForDept.Count; idx++)
                    {
                        wsDept.Cells["A" + (idx + 2)].Value = DataForDept[idx];
                    }

                    var wsLocation = package.Workbook.Worksheets[3];
                    List<string> DataLocation = User_Info._GetLocation();
                    for (int idx = 0; idx < DataLocation.Count; idx++)
                    {
                        wsLocation.Cells["A" + (idx + 2)].Value = DataLocation[idx];
                    }

                    package.Save();
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(tempPath);
                System.IO.File.Delete(tempPath);
                return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "FormatTemplateRequest.xlsx"
            );
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi xử lý: {ex.Message}");
            }

        }

        [HttpPost]
        public async Task<IActionResult> ImportFileExcel(IFormFile file)
        {
            //if(file == null || file.Length = 0)
            //{
            //    return BadRequest("Chưa lựa chọn file");
            //}
            //var extension = Path.GetExtension(file.FileName).ToLower();
            //if (extension != ".xlsx") return BadRequest("Định dạng file không hỗ trợ. Vui lòng upload file Excel!");
            //string folderPath = Path.Combine(_env.ContentRootPath, "DataUpload", "Uploads");
            //string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            //string filePath = Path.Combine(folderPath, uniqueFileName);
            //using (var stream = new FileStream(filePath, FileMode.Create))
            //{
            //    await file.CopyToAsync(stream);
            //}

            var resultList = new List<Dictionary<string, object>>();

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx") return BadRequest("Định dạng file không hỗ trợ. Vui lòng upload file Excel!");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using (var package = new ExcelPackage(stream))
                {
                    var wsFormat = package.Workbook.Worksheets[0];
                    var rowCount = wsFormat.Dimension?.Rows ?? 0;
                    if (rowCount < 2) return Json(new { message = "File Empty" });
                    var headers = new List<string>()
                    {
                        "id",
                        "nameMaterial",
                        "quantity",
                        "purpose",
                        "deptCost",
                        "location",
                        "notetake"
                    };
                    //"costKT",
                    //    "warehouse",
                    //    "stock",
                    //    "unit",
                    //    "price",
                    //    "typePay"

                    for (int idx = 2; idx <= rowCount; idx++)
                    {
                        string nameMaterial = wsFormat.Cells["B" + idx].Text;
                        if (nameMaterial.Trim() == "") continue;
                        var rowData = new Dictionary<string, object>();
                        for (int i = 0; i < headers.Count; i++)
                        {
                            var key = headers[i];
                            var value = wsFormat.Cells[idx, i + 1].Text;
                            rowData[key] = value;
                        }
                        var dataOther = MATERIA.material_process(new PARAS()
                        {
                            Material_Code = nameMaterial
                        });

                        if (dataOther.Count == 1)
                        {
                            rowData["costKT"] = dataOther.First().Account_Code!.ToString();
                            rowData["warehouse"] = dataOther.First().Inventory!.ToString();
                            rowData["stock"] = dataOther.First().Num_Inventory!.ToString();
                            rowData["unit"] = dataOther.First().Unit!.ToString();
                            rowData["price"] = dataOther.First().Price.ToString();
                            rowData["typePay"] = "USD";
                        }

                        resultList.Add(rowData);
                    }
                }
            }
            return Json(resultList);
        }

        [HttpPost]
        public ActionResult ExportModalDetail(string code_request)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var list = REQUEST_PROCESS._load_body_detail(code_request);
            var load = REQUEST_PROCESS._load_request(code_request);

            string pathDir = Path.Combine(_env.ContentRootPath, "Data");
            string templatePath = Path.Combine(pathDir, "Template_HangTrongDanhMuc.xlsm");
            string tempFileName = $"HangTrongDanhMuc_{Guid.NewGuid()}.xlsm";
            string tempPath = Path.Combine(pathDir, tempFileName);

            try
            {
                System.IO.File.Copy(templatePath, tempPath, true);
                FileInfo fileInfo = new FileInfo(tempPath);
                using (var package = new ExcelPackage(fileInfo))
                {
                    var ws = package.Workbook.Worksheets[0];
                    ws.Cells["L2"].Value = load.First().Group_Code;
                    ws.Cells["A5"].Value = load.First().Cost_Center_Group;
                    ws.Cells["C5"].Value = load.First().Cost_Center;
                    ws.Cells["D5"].Value = load.First().Name;
                    ws.Cells["E5"].Value = load.First().Place;
                    ws.Cells["F5"].Value = load.First().Create_Date;
                    ws.Cells["G5"].Value = load.First().Dealine.Split(' ')[0];

                    for (int idx = 1; idx <= 12; idx++)
                    {
                        if (list.Count < idx) break;
                        ws.Cells["B" + (8 + idx)].Value = list[idx - 1].Material_Code;
                        ws.Cells["C" + (8 + idx)].Value = list[idx - 1].Material_Name;
                        //ws.Cells["E" + (8 + idx)].Value = list[idx].Unit;
                        ws.Cells["F" + (8 + idx)].Value = list[idx - 1].Account_Code;
                        ws.Cells["G" + (8 + idx)].Value = list[idx - 1].Account_Name;
                        ws.Cells["H" + (8 + idx)].Value = list[idx - 1].Amount;
                        ws.Cells["I" + (8 + idx)].Value = list[idx - 1].Unit;
                        ws.Cells["J" + (8 + idx)].Value = list[idx - 1].Price;
                        ws.Cells["K" + (8 + idx)].Value = load.First().Group_Code.Contains("GA") ? "VND" : "USD";
                        ws.Cells["M" + (8 + idx)].Value = list[idx - 1].Aim;
                        //ws.Cells["O" + (8 + idx)].Value = list[idx].;
                    }

                    package.Save();
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(tempPath);
                System.IO.File.Delete(tempPath);
                return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"HangTrongDanhMuc_{code_request}.xlsm"
            );
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi xử lý: {ex.Message}");
            }

        }
    }
}
