using DocumentFormat.OpenXml.Office2021.Drawing.SketchyShapes;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Reflection;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class WarehouseTransferRequest
    {
        public string? MaNguyenLieu { get; set; }
        public double? SoLuong { get; set; }
        public string? KhoChuyen { get; set; }
        public string? KhoNhan { get; set; }
        public string? Khoi { get; set; }
        public string? ViTriRaw { get; set; } // Tương ứng cmbVitri.SelectedItem
        public DateTime NgayNhapKho { get; set; }
        public double SoLuongHienTaiTaiKhoChuyen { get; set; }
    }  
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
        public JsonResult _load_material(string group_code, string loaichiphi)
        {
            string mahang = "";
            if (loaichiphi == "AUXILIARY")
            {
                mahang = "E";
            }
            if (loaichiphi == "EUQIMENT")
            {
                mahang = "A";
            }
            if (group_code == "GA")
            {
                mahang = "B";
            }
            if (group_code.Contains("_CPK"))
            {
                mahang = "";
                group_code = group_code.Split("_")[0];
            }
            List<string> material = MST_INVENTORY._getname_material(group_code, mahang);
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
        public JsonResult _get_log(string madon, string ngay_tu, string ngay_den, string kho, string manguyenlieu, string loai, string us)
        {
            List<Models.KHO_NHAPXUAT> lst = Models.KHO_NHAPXUAT._logg(madon, ngay_tu, ngay_den, kho, manguyenlieu, loai, us);
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
            string templatePath = Path.Combine(path, "Form_Export_WH.xlsx");
            string tempFileName = $"Temp_{Guid.NewGuid()}.xlsx";
            string tempPath = Path.Combine(path, tempFileName);

            try
            {
                System.IO.File.Copy(templatePath, tempPath, true);
                FileInfo fileInfo = new FileInfo(tempPath);
                using (var package = new ExcelPackage(fileInfo))
                {
                    var wsMaterial = package.Workbook.Worksheets.First();
                    List<MST_INVENTORY> DataListMaterial = MST_INVENTORY.exportExcel_kho();
                    for (int i = 0; i < DataListMaterial.Count; i++)
                    {
                        wsMaterial.Cells[i + 2, 1].Value = i + 1;
                        wsMaterial.Cells[i + 2, 2].Value = DataListMaterial[i].MaNguyenLieu;
                        wsMaterial.Cells[i + 2, 3].Value = DataListMaterial[i].Material_Name;
                        wsMaterial.Cells[i + 2, 4].Value = DataListMaterial[i].Unit;
                        wsMaterial.Cells[i + 2, 5].Value = DataListMaterial[i].Hientai;
                        wsMaterial.Cells[i + 2, 6].Value = DataListMaterial[i].Group_Code;
                        wsMaterial.Cells[i + 2, 7].Value = DataListMaterial[i].Kho;
                        wsMaterial.Cells[i + 2, 8].Value = DataListMaterial[i].DTM_UPDATE;

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
            var Kho = _db.GET_DATA_FROM_SQL(" select Kho, Hientai from Kho where MaNguyenLieu = '" + mahang + "' and Kho = '" + kho + "'");
            string soluong = Kho.Rows[0]["Hientai"].ToString()!;
            return Json(soluong);
        }
        public JsonResult _xuatkhothucte(string code_request, string adid_nx, string nguoinhan, string nguoixuatkho, DateTime thoigian, string manguyenlieu, string soluong, string giathucte, string donvi, string kho, string tongchiphi, string vitri, string phong, string khoi, string tongchiphiold, string id_rq)
        {           
            var check = REQUEST_PROCESS._xuatkho(code_request, adid_nx, nguoinhan, nguoixuatkho, thoigian, manguyenlieu, soluong, giathucte, donvi, kho, tongchiphi, vitri, phong, khoi, id_rq);
            return Json(check);
        }
        public JsonResult _load_xuatkhohang(string us, string mayeucau, string nguoitao, string khoi, string thangnam)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            var chk = db.ReturnString($"select [Group_Code] from PE_USERNAME where Adid = '{us}'");

            if (string.IsNullOrEmpty(thangnam))
            {
                thangnam = DateTime.Now.ToString("yyyy-M");
            }
            string thang = thangnam.Split('-')[1];
            string nam = thangnam.Split('-')[0];
          
            if (thang.Length == 2)
            {
                thang = thang.Substring(1);
            }
            if (chk == "GA")
            {
                var list = Models.REQUEST_PROCESS_GA._load_tonkhoxuathang(mayeucau, nguoitao, khoi, thang, nam);
                return Json(list);
            }

            if (chk == "PROD")
            {
                
                var list = Models.REQUEST_PROCESS._load_tonkhoxuathang(us, mayeucau, nguoitao, khoi, thang, nam);
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
            var data = new List<Models.KHO_NHAPXUAT> {
            new Models.KHO_NHAPXUAT {  }
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
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
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
                        wsMaterial.Cells["A" + (idx + 2)].Value = DataListMaterial[idx].Split(":")[0];
                        wsMaterial.Cells["B" + (idx + 2)].Value = DataListMaterial[idx].Split(":")[1];
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

                    var wsAccCost = package.Workbook.Worksheets[4];

                    var list = db.GET_DATA_FROM_SQL("Select null As Account_Code,null As Account_Name_EN Union ALL SELECT DISTINCT [Account_Code],[Account_Code]+':'+[Account_Name_EN] AS Account_Name_EN FROM [TM_ACCOUNT]");
                    List<string> maketoan = new List<string>();
                    for (int i = 0; i < list.Rows.Count; i++)
                    {
                        maketoan.Add(list.Rows[i][1].ToString()!);
                    }
                    for (int idx = 0; idx < maketoan.Count; idx++)
                    {
                        wsAccCost.Cells["A" + (idx + 2)].Value = maketoan[idx];
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
        public async Task<IActionResult> ImportFileExcel(IFormFile file, string us, string khoi)
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

            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            //string khoi = db.ReturnString("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[GROUP_MEMBER] WHERE [CHR_USERID] = '" + us + "'");
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
                        "stk",
                        "quantity",
                        "purpose",
                        "deptCost",
                        "location",
                        "notetake"
                    };
                  
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
                            Material_Code = nameMaterial,
                            Group_Code = khoi
                        });

                        if (dataOther.Count >= 1)
                        {
                            rowData["tenht"] = dataOther.First().Material_Code!.ToString() + ":" + dataOther.First().Material_Name_VN!.ToString();
                            rowData["costKT"] = dataOther.First().Account_Code!.ToString() + ":" + dataOther.First().Account_Name_EN!.ToString();
                            rowData["warehouse"] = dataOther.First().Inventory!.ToString();
                            rowData["stock"] = dataOther.First().Num_Inventory!.ToString();
                            rowData["unit"] = dataOther.First().Unit!.ToString();
                            rowData["price"] = dataOther.First().Price.ToString()!;
                            rowData["typePay"] = dataOther.First().Currency!.ToString()!;
                            resultList.Add(rowData);
                        }
                    }
                }
            }
            return Json(resultList);
        }
        [HttpPost]
        public ActionResult ExportModalDetail(string code_request)
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            code_request = code_request.Replace("*", "");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var list = REQUEST_PROCESS._load_body_detail_export(code_request);
            var load = REQUEST_PROCESS._load_request(code_request);
            var checklist = sql.GET_DATA_FROM_SQL("select * from REQUEST where Code_Request = '" + code_request + "'");
            var get_id = checklist.Rows[0]["Id_Request"].ToString();
            var nguoixuat = checklist.Rows[0]["User_Update"].ToString() + " \n " + checklist.Rows[0]["Last_Update"].ToString()!.Split(' ')[0];
            var get_adid = sql.GET_DATA_FROM_SQL("select * from [PE_REQUEST_CONFIRM] where ID_REQUEST = '" + get_id + "' and ( INT_STEP >= 3 and INT_STEP <= 5 OR INT_STEP = 11)");
           
            string nguoilamdon = "";
            string nguoitao = "";
            string qltc = "";
            string qlcc = "";
            string xuatkho = "";
            string quanlytiepnhan = "";
            string dongy = "";
            if (get_adid.Rows.Count > 0)
            {
                 nguoitao = get_adid.Rows[0]["CHR_ADID_NGUOITAO"].ToString()! + " \n "  + checklist.Rows[0]["Create_Date"].ToString()!.Split(' ')[0];
                 nguoilamdon = get_adid.Rows[0]["CHR_ADID_NGUOIYEUCAU"].ToString()! + " \n " + get_adid.Rows[0]["DTM_NGUOIYEUCAU"].ToString()!.Split(' ')[0];
                 qltc = get_adid.Rows[0]["CHR_ADID_NGUOITHAMTRA"].ToString()! + " \n " + get_adid.Rows[0]["DTM_NGUOITHAMTRA"].ToString()!.Split(' ')[0];
                 qlcc = get_adid.Rows[0]["CHR_ADID_NGUOIPHEDUYET"].ToString()! + " \n " + get_adid.Rows[0]["DTM_NGUOITHAMTRA"].ToString()!.Split(' ')[0];
                 xuatkho = get_adid.Rows[0]["CHR_ADID_XUATKHO"].ToString()! + " \n " + get_adid.Rows[0]["DTM_XACNHAN"].ToString()!.Split(' ')[0];
                 quanlytiepnhan = get_adid.Rows[0]["CHR_ADID_XACNHAN"].ToString()! + " \n " + get_adid.Rows[0]["DTM_XACNHAN"].ToString()!.Split(' ')[0];
                 dongy = get_adid.Rows[0]["CHR_ADID_XUATKHO"].ToString()! + " \n " + get_adid.Rows[0]["DTM_XACNHAN"].ToString()!.Split(' ')[0];
            }
            else
            {
                 get_adid = sql.GET_DATA_FROM_SQL("select * from [PE_REQUEST_CONFIRM_GA] where ID_REQUEST = '" + get_id + "' and ( INT_STEP >= 3 and INT_STEP <= 5  OR INT_STEP = 11) ");
                 if(get_adid.Rows.Count > 0)
                 {
                    nguoitao = get_adid.Rows[0]["CHR_ADID_NGUOITAO"].ToString()! + " \n " + checklist.Rows[0]["Create_Date"].ToString()!.Split(' ')[0];
                    nguoilamdon = get_adid.Rows[0]["CHR_ADID_NGUOIYEUCAU"].ToString()! + " \n " + get_adid.Rows[0]["DTM_NGUOIYEUCAU"].ToString()!.Split(' ')[0];
                    qltc = get_adid.Rows[0]["CHR_ADID_NGUOITHAMTRA"].ToString()! + " \n " + get_adid.Rows[0]["DTM_NGUOITHAMTRA"].ToString()!.Split(' ')[0];
                    qlcc = get_adid.Rows[0]["CHR_ADID_NGUOIPHEDUYET"].ToString()! + " \n " + get_adid.Rows[0]["DTM_NGUOITHAMTRA"].ToString()!.Split(' ')[0];
                    xuatkho = get_adid.Rows[0]["CHR_ADID_XUATKHO"].ToString()! + " \n " + get_adid.Rows[0]["DTM_QLSC"].ToString()!.Split(' ')[0];
                    quanlytiepnhan = get_adid.Rows[0]["CHR_ADID_QLTC"].ToString()! + " \n " + get_adid.Rows[0]["DTM_QLTC"].ToString()!.Split(' ')[0];
                    dongy = get_adid.Rows[0]["CHR_ADID_QLSC"].ToString()! + " \n " + get_adid.Rows[0]["DTM_QLSC"].ToString()!.Split(' ')[0];
                 }
            }
            
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
                    var firstLoad = load.First();

                    // Điền thông tin header
                    ws.Cells["L2"].Value = firstLoad.Code_Request;
                    ws.Cells["J2"].Value = firstLoad.Group_Code;
                    ws.Cells["A5"].Value = firstLoad.Cost_Center_Group;
                    ws.Cells["C5"].Value = firstLoad.Cost_Center;
                    ws.Cells["D5"].Value = firstLoad.Name;
                    ws.Cells["E5"].Value = firstLoad.Place;
                    ws.Cells["F5"].Value = firstLoad.Create_Date;
                    ws.Cells["G5"].Value = firstLoad.Dealine?.Split(' ')[0];

                    int startRow = 9; // Dòng bắt đầu điền dữ liệu (ứng với idx=1 là 8+1)
                    int totalItems = list.Count;
                    int kiten = 24;
                    // 1. Nếu danh sách > 12, chèn thêm dòng
                    if (totalItems > 12)
                    {
                        ws.InsertRow(startRow + 12, totalItems - 12, startRow + 11);
                        kiten = kiten + totalItems;
                    }

                    // 2. Lặp qua toàn bộ danh sách (không giới hạn 12 nữa)
                    for (int i = 0; i < totalItems; i++)
                    {
                        int currentRow = startRow + i;
                        var item = list[i];
                        ws.Cells["A" + currentRow].Value = i + 1;
                        ws.Cells["B" + currentRow].Value = item.Material_Code;
                        ws.Cells["C" + currentRow].Value = item.Material_Name;
                        ws.Cells["F" + currentRow].Value = item.Account_Code;
                        ws.Cells["G" + currentRow].Value = item.Account_Name;
                        ws.Cells["H" + currentRow].Value = item.Amount;
                        ws.Cells["I" + currentRow].Value = item.Unit;
                        ws.Cells["J" + currentRow].Value = item.Price;
                        ws.Cells["K" + currentRow].Value = firstLoad.Group_Code!.Contains("GA") ? "VND" : "USD";
                        ws.Cells["L" + currentRow].Formula = (item.Amount * item.Price).ToString();
                        ws.Cells["M" + currentRow].Value = item.Aim;
                    }

                    int totalRow = startRow + totalItems; // Dòng ngay sau dòng dữ liệu cuối cùng                                       
                    ws.Cells["L" + (totalRow + 12)].Formula = $"=SUM(L{startRow}:L{totalRow - 1})";
                    ws.Calculate();

                    ws.Cells["O5"].Value = nguoitao;
                    ws.Cells["K5"].Value = qltc;
                    ws.Cells["H5"].Value = qlcc;

                    ws.Cells["D" + kiten].Value = nguoixuat;
                    ws.Cells["A" + kiten].Value = xuatkho;
                    ws.Cells["C" + kiten].Value = quanlytiepnhan;
                    ws.Cells["E" + kiten].Value = nguoitao.Split(' ')[0] +" \n " + checklist.Rows[0]["Last_Update"].ToString()!.Split(' ')[0]; ;
                    string checkgia = string.IsNullOrEmpty(firstLoad.Total_Real) ? "0" : firstLoad.Total_Real;
                    if (float.Parse(checkgia) >= 10000)
                    {
                        ws.Cells["M5"].Value = nguoilamdon;                           
                    }

                    // Tính toán lại toàn bộ công thức trong sheet trước khi lưu
                    ws.Calculate();
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
        [HttpPost]
        public JsonResult chuyenkho(string malinhkien, string soluonghientai, string khochuyen, string khonhan, string vitri, string soluongchuyen, string ngaychuyen, string us)
        {
            if (string.IsNullOrEmpty(ngaychuyen) || ngaychuyen == "1/1/0001 12:00:00 AM" || ngaychuyen.Contains("1900-01-01"))
            {
                ngaychuyen = DateTime.Now.ToString();
            }
            var chuyenkho = MST_INVENTORY._chuyenkho(malinhkien, soluonghientai, khochuyen, khonhan, vitri, soluongchuyen, ngaychuyen, us);
            return Json(chuyenkho);
        }
        [HttpPost]
        public JsonResult _load_account()
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            var list = db.GET_DATA_FROM_SQL("Select null As Account_Code,null As Account_Name_EN Union ALL SELECT DISTINCT [Account_Code],[Account_Code]+':'+[Account_Name_EN] AS Account_Name_EN FROM [TM_ACCOUNT]");
            List<string> maketoan = new List<string>();
            for (int i = 0; i < list.Rows.Count; i++)
            {
                maketoan.Add(list.Rows[i][1].ToString()!);
            }
            return Json(maketoan);
        }
        [HttpPost]
        public JsonResult _XOADONPROD(string iD_REQUEST)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();

            //var xoatrangthai = db.GET_DATA_FROM_SQL("delete from [PE_REQUEST_CONFIRM] where  [ID_REQUEST] = '" + iD_REQUEST + "'");
            //var xoa_request = db.GET_DATA_FROM_SQL("delete from [REQUEST] where  [Id_Request] = '" + iD_REQUEST + "' ");
            //var xoa_deltail = db.GET_DATA_FROM_SQL("delete from [REQUEST_DETAIL] where  [Id_Request] = '" + iD_REQUEST + "' ");
            // check trạng thái đơn
            var list = db.ReturnString("select count(*) from [PE_REQUEST_CONFIRM] where INT_STEP = '0' and  [ID_REQUEST] = '" + iD_REQUEST + "' ");
            try
            {
                if (list != "0")
                {
                    var xoatrangthai = db.GET_DATA_FROM_SQL("delete from [PE_REQUEST_CONFIRM] where  [ID_REQUEST] = '" + iD_REQUEST + "'");
                    var xoa_request = db.GET_DATA_FROM_SQL("delete from [REQUEST] where  [Id_Request] = '" + iD_REQUEST + "' ");
                    var xoa_deltail = db.GET_DATA_FROM_SQL("delete from [REQUEST_DETAIL] where  [Id_Request] = '" + iD_REQUEST + "' ");
                    return Json("Xóa đơn thành công");
                }
                else
                {
                    return Json("Không thành công . Kiểm tra trạng thái đơn");
                }
            }
            catch
            {
                return Json("Đơn yêu cầu lỗi");
            }
        }
        [HttpPost]
        public JsonResult _XOADONGA(string iD_REQUEST)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            // check trạng thái đơn
            var list = db.ReturnString("select count(*) from [PE_REQUEST_CONFIRM_GA] where INT_STEP = '0' and  [ID_REQUEST] = '" + iD_REQUEST + "' ");
            try
            {
                if (list != "0")
                {
                    var xoatrangthai = db.GET_DATA_FROM_SQL("delete from [PE_REQUEST_CONFIRM_GA] where  [ID_REQUEST] = '" + iD_REQUEST + "'");
                    var xoa_request = db.GET_DATA_FROM_SQL("delete from [REQUEST] where  [Id_Request] = '" + iD_REQUEST + "' ");
                    var xoa_deltail = db.GET_DATA_FROM_SQL("delete from [REQUEST_DETAIL] where  [Id_Request] = '" + iD_REQUEST + "' ");
                    return Json("Xóa đơn thành công");
                }
                else
                {
                    return Json("Không thành công do đơn thay đổi trạng thái");
                }
            }
            catch
            {
                return Json("Đơn yêu cầu lỗi");
            }
        }
        public List<Models.REQUEST_DETAIL> GetListRequestDetail(string iD_REQUEST)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            //var request = db.GET_DATA_FROM_SQL("select * from DETAIL where Id_Request = '" + iD_REQUEST + "'");
            //List<REQUEST> rq = new List<REQUEST>();

            var request_detail = db.GET_DATA_FROM_SQL("select * from [REQUEST_DETAIL] where Id_Request = '" + iD_REQUEST + "'");
            var get_vitri = db.ReturnString("select Place from [REQUEST] where [Id_Request] = '" + iD_REQUEST + "'");

           
            List<Models.REQUEST_DETAIL> rq_dt = new List<Models.REQUEST_DETAIL>();

            for (int i = 0; i < request_detail.Rows.Count; i++)
            {
                var get_phongban = db.ReturnString("select [Name] from DEPARTMENT as a left join REQUEST_DETAIL as b on a.Cost_Center = b.Phongchiuchiphi where Id_RequestDetail = '" + request_detail.Rows[i]["Id_RequestDetail"].ToString() + "'");

                rq_dt.Add(new Models.REQUEST_DETAIL
                {
                    Material_Name = request_detail.Rows[i]["Material_Code"].ToString() + ":" + request_detail.Rows[i]["Material_Name"].ToString(),
                    Account_Code = request_detail.Rows[i]["Account_Code"].ToString() + ":" + request_detail.Rows[i]["Account_Name"].ToString(),
                    Amount = float.Parse(request_detail.Rows[i]["Amount"].ToString()!),
                    Unit = request_detail.Rows[i]["Unit"].ToString(),
                    Price = float.Parse(request_detail.Rows[i]["Price"].ToString()!),
                    Currency = request_detail.Rows[i]["Currency"].ToString(),
                    Total_exchange = float.Parse(request_detail.Rows[i]["Total_exchange"].ToString()!),
                    Aim = request_detail.Rows[i]["Aim"].ToString(),
                    Phongchiuchiphi = request_detail.Rows[i]["Phongchiuchiphi"].ToString() + ":" + get_phongban.ToString(),
                    Vitri = request_detail.Rows[i]["Vitri"].ToString(),
                    Poisition = request_detail.Rows[i]["Poisition"].ToString(),
                    Id_RequestDetail = int.Parse(request_detail.Rows[i]["Id_RequestDetail"].ToString()!)
                });
            }
            return rq_dt;
        }
        public JsonResult request(string iD_REQUEST)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            //var request = db.GET_DATA_FROM_SQL("select * from DETAIL where Id_Request = '" + iD_REQUEST + "'");
            //List<REQUEST> rq = new List<REQUEST>();

            var request_detail = db.GET_DATA_FROM_SQL("select * from [REQUEST] where Id_Request = '" + iD_REQUEST + "'");
            List<Models.REQUEST> rq_dt = new List<Models.REQUEST>();
            var get_namesec = request_detail.Rows[0]["Cost_Center"].ToString();
            for (int i = 0; i < request_detail.Rows.Count; i++)
            {
                rq_dt.Add(new Models.REQUEST
                {
                    Code_Request = request_detail.Rows[i]["Code_Request"].ToString(),
                    Cost_Center = request_detail.Rows[i]["Cost_Center"].ToString(),
                    Request_Date = request_detail.Rows[i]["Request_Date"].ToString(),
                    Declaration = request_detail.Rows[i]["Declaration"].ToString(),
                    Dealine = Convert.ToDateTime(request_detail.Rows[i]["Dealine"]).ToString("yyyy-MM-dd"),
                    Total_exchange = float.Parse(request_detail.Rows[i]["Total_exchange"].ToString()!),
                    Exchange_rate = request_detail.Rows[i]["Exchange_rate"].ToString(),
                    Currency = request_detail.Rows[i]["Currency"].ToString(),
                    Total = float.Parse(request_detail.Rows[i]["Total"].ToString()!),
                    Typee = request_detail.Rows[i]["Type"].ToString(),
                    Loaihinhtokhai = request_detail.Rows[i]["Loaihinhtokhai"].ToString(),
                    Group_Code = request_detail.Rows[i]["Group_Code"].ToString(),
                    Urgent = request_detail.Rows[i]["Urgent"].ToString(),
                    Place = request_detail.Rows[i]["Place"].ToString(),
                });
            }
           
            return Json(rq_dt);
        }
        public ActionResult Suadon(string iD_REQUEST)
        {
            var model = GetListRequestDetail(iD_REQUEST); // Danh sách chi tiết (IEnumerable)
            ViewBag.MasterData = iD_REQUEST;      // Thông tin tổng quát đơn hàng
            return View(model);
        }
        [HttpPost]
        public JsonResult NhapKhoDacBiet([FromBody] NhapKhoRequest request)
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            string Manhanvien = sql.ReturnString("SELECT CHR_CRT_USERID FROM [TM_USER] WHERE [CHR_USERID] = '" + request.us + "' ");
            string Soluonghientai = sql.ReturnString("SELECT [Hientai] FROM KHO WHERE [MaNguyenLieu] =  N'" + request.MaNguyenLieu + "' AND [Kho] = '" + request.Kho + "' ");
            double SoluongTruocthaydoi = 0;
            if (Soluonghientai.Trim() == "")
            {
                sql.GET_DATA_FROM_SQL("INSERT INTO KHO(MaNguyenLieu,Hientai,Group_Code,Kho) VALUES (N'" + request.MaNguyenLieu!.Trim().ToUpper() + "','" + request.Soluong + "','" + request.Khoi + "','" + request.Kho + "')");
            }
            else
            {
                sql.GET_DATA_FROM_SQL("UPDATE KHO SET [Hientai] = [Hientai] + " + request.Soluong + " WHERE [MaNguyenLieu] =  N'" + request.MaNguyenLieu!.Trim() + "' AND [Kho] = '" + request.Kho + "' AND [Group_Code] = '" + request.Khoi + "'");
                SoluongTruocthaydoi = Convert.ToDouble(Soluonghientai);
            }          
            var Nguyenlieu = sql.GET_DATA_FROM_SQL("SELECT * FROM [MATERIAL] WHERE [Material_Code] = '" + request.MaNguyenLieu.Trim() + "' ");
            sql.GET_DATA_FROM_SQL("INSERT INTO [KHO_NHAPXUAT]([MaNguyenLieu],[Hanhdong],[Soluong],[Loai],[Thoigian],[Nguoicapnhat],[Kho],[Khoi],[TenNguyenlieu],[Donvi],[MaNguoinhap],[Gia],[Ngaynhaokho],[Soluongtruocthaydoi],[Soluongsauthaydoi]) VALUES(N'" + request.MaNguyenLieu.Trim().ToUpper() + "',N'Nhập hàng đặc biệt vào kho " + request.Kho + ", Ghi chú: " + request.GhiChu + "','" + Convert.ToDouble(request.Soluong.ToString()!.Trim()) + "','NHAP','" + request.NgayNhapKho.ToString("MM/dd/yyyy HH:mm:ss") + "','" + request.us + "','" + request.Kho + "','" + request.Khoi + "',N'" + Nguyenlieu.Rows[0]["Material_Name_JP"].ToString()!.Trim() + "',N'" + Nguyenlieu.Rows[0]["Unit"].ToString()!.Trim() + "','" + Manhanvien + "','0','" + request.NgayNhapKho.ToString("MM/dd/yyyy HH:mm:ss") + "','" + SoluongTruocthaydoi + "','" + (Convert.ToDouble(request.Soluong.ToString()!.Trim()) + SoluongTruocthaydoi) + "')");

            return Json("OK");
        }
    }
    public class NhapKhoRequest
    {
        public string? MaNguyenLieu { get; set; }
        public decimal? Soluong { get; set; }
        public string? Kho { get; set; }
        public DateTime NgayNhapKho { get; set; }
        public string? GhiChu { get; set; }
        public string? Khoi { get; set; }
        public string? us { get; set; }
    }
}
