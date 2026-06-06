using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using PRJ_WAREHOUSE_BIVN.Models;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Xml.Linq;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class PE_UYQUYEN
    {
        public string? CHR_ADID_NguoiduocUQ { get; set; }
        public string? CHR_MAIL_NguoiduocUQ { get; set; }
        public string? CHR_TEN_NguoiduocUQ { get; set; }
        public string? DT_HethanUQ { get; set; }
    }
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public PartialViewResult _modal()
        {
            return PartialView();
        }
        [HttpPost]
        public JsonResult Load_User(Search_param para)
        {
            List<MST_USER> dt = MST_USER.user_process(para );
            return Json(dt);
        }
        [HttpPost]
        public JsonResult Insert_Edit_User(string name, string adid, string staffcode, string dept, string role, string mail)      
        {
           string result =  MST_USER.insert_update_users(name, adid, staffcode, dept, role, mail, adid, "1");
           return Json(result);
        }
        public ActionResult ThemUser_Phongtiepnhan()
        {
            return View();
        }
        public JsonResult Load_UQ(string adid)
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            List<PE_UYQUYEN> lst = new List<PE_UYQUYEN>();
            var uyquyen = sql.GET_DATA_FROM_SQL("select * from PE_UYQUYEN where CHR_ADID_NguoiUQ = '" + adid + "' and DT_HethanUQ >= GETDATE()");
            for (int i = 0; i < uyquyen.Rows.Count; i++)
            {
                lst.Add(new PE_UYQUYEN
                {
                    CHR_ADID_NguoiduocUQ = uyquyen.Rows[i]["CHR_ADID_NguoiduocUQ"].ToString(),
                    CHR_MAIL_NguoiduocUQ = uyquyen.Rows[i]["CHR_MAIL_NguoiduocUQ"].ToString(),
                    CHR_TEN_NguoiduocUQ = uyquyen.Rows[i]["CHR_TEN_NguoiduocUQ"].ToString(),
                    DT_HethanUQ = uyquyen.Rows[i]["DT_HethanUQ"].ToString()
                });
            }
            return Json(lst);
        }
        public ActionResult QuanlyUyquyen()
        {
            return View();
        }
        public class UyQuyenModel
        {
            public int ID { get; set; }
            public string? CHR_ADID_NguoiUQ { get; set; }
            public string? CHR_ADID_NguoiduocUQ { get; set; }
            public string? CHR_MAIL_NguoiduocUQ { get; set; }
            public string? CHR_TEN_NguoiduocUQ { get; set; }
            public string? DT_HethanUQ { get; set; }
            public string? CHR_PHONGBAN { get; set; }
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            var list = new List<UyQuyenModel>();

            // 1. Tạo câu truy vấn
            string query = @"SELECT [ID], [CHR_ADID_NguoiUQ], [CHR_ADID_NguoiduocUQ], 
                            [CHR_MAIL_NguoiduocUQ], [CHR_TEN_NguoiduocUQ], 
                            [DT_HethanUQ], [CHR_PHONGBAN] 
                     FROM [COST_MANAGEMENT].[dbo].[PE_UYQUYEN]";

        
            var dt = sql.GET_DATA_FROM_SQL(query);

            // 4. Chuyển đổi từ DataTable sang List<Model>
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new UyQuyenModel
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        CHR_ADID_NguoiUQ = row["CHR_ADID_NguoiUQ"].ToString()!,
                        CHR_ADID_NguoiduocUQ = row["CHR_ADID_NguoiduocUQ"].ToString()!,
                        CHR_MAIL_NguoiduocUQ = row["CHR_MAIL_NguoiduocUQ"].ToString()!,
                        CHR_TEN_NguoiduocUQ = row["CHR_TEN_NguoiduocUQ"].ToString()!,
                        DT_HethanUQ = row["DT_HethanUQ"].ToString()!,
                        CHR_PHONGBAN = row["CHR_PHONGBAN"].ToString()!
                    });
                }
            }
           
            return Json(list);
        }

        // 1. THÊM MỚI (CREATE)
        [HttpPost]
        public IActionResult Create([FromBody] UyQuyenModel model)
        {
            if (model == null) return BadRequest("Dữ liệu không hợp lệ");

            SQL_Connect_DB20 sql = new SQL_Connect_DB20();

            // Xây dựng câu truy vấn INSERT
            string query = $@"
            INSERT INTO [COST_MANAGEMENT].[dbo].[PE_UYQUYEN] 
            (
                [CHR_ADID_NguoiUQ], 
                [CHR_ADID_NguoiduocUQ], 
                [CHR_MAIL_NguoiduocUQ], 
                [CHR_TEN_NguoiduocUQ], 
                [DT_HethanUQ], 
                [CHR_PHONGBAN]
            )
            VALUES 
            (
                N'{model.CHR_ADID_NguoiUQ}', 
                N'{model.CHR_ADID_NguoiduocUQ}', 
                N'{model.CHR_MAIL_NguoiduocUQ}', 
                N'{model.CHR_TEN_NguoiduocUQ}', 
                '{model.DT_HethanUQ}', 
                N'{model.CHR_PHONGBAN}'
            )";

            try
            {
                // LƯU Ý: Thay "EXECUTE_SQL" bằng tên hàm chạy lệnh Insert/Update/Delete trong class SQL_Connect_DB20 của bạn
                sql.GET_DATA_FROM_SQL(query);
                return Json(new { success = true, message = "Thêm thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi thêm: " + ex.Message });
            }
        }

        // 2. CHỈNH SỬA (UPDATE)
        [HttpPost]
        public IActionResult Update([FromBody] UyQuyenModel model)
        {
            if (model == null || model.ID <= 0) return BadRequest("Dữ liệu không hợp lệ");

            SQL_Connect_DB20 sql = new SQL_Connect_DB20();

            // Xây dựng câu truy vấn UPDATE
            string query = $@"
            UPDATE [COST_MANAGEMENT].[dbo].[PE_UYQUYEN]
            SET 
                [CHR_ADID_NguoiUQ] = N'{model.CHR_ADID_NguoiUQ}',
                [CHR_ADID_NguoiduocUQ] = N'{model.CHR_ADID_NguoiduocUQ}',
                [CHR_MAIL_NguoiduocUQ] = N'{model.CHR_MAIL_NguoiduocUQ}',
                [CHR_TEN_NguoiduocUQ] = N'{model.CHR_TEN_NguoiduocUQ}',
                [DT_HethanUQ] = '{model.DT_HethanUQ}',
                [CHR_PHONGBAN] = N'{model.CHR_PHONGBAN}'
            WHERE [ID] = {model.ID}";

            try
            {
                // LƯU Ý: Thay "EXECUTE_SQL" bằng tên hàm tương ứng của bạn
                sql.GET_DATA_FROM_SQL(query);
                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi cập nhật: " + ex.Message });
            }
        }

        // 3. XÓA (DELETE)
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (id <= 0) return BadRequest("ID không hợp lệ");

            SQL_Connect_DB20 sql = new SQL_Connect_DB20();

            // Xây dựng câu truy vấn DELETE
            string query = $"DELETE FROM [COST_MANAGEMENT].[dbo].[PE_UYQUYEN] WHERE [ID] = {id}";

            try
            {
                // LƯU Ý: Thay "EXECUTE_SQL" bằng tên hàm tương ứng của bạn
                sql.GET_DATA_FROM_SQL(query);
                return Json(new { success = true, message = "Xóa thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }
    }
}
