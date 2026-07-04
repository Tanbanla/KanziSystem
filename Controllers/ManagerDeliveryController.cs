using Dapper;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.Models;
using System.Data.SqlClient;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class PoViewModel
    {
        public int Id { get; set; } // Số thứ tự hoặc ID
        public DateTime ReqDate { get; set; }
        public DateTime DeliveryReqDate { get; set; }
        public string? PoNumber { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public int Quantity { get; set; }
        public string? Unit { get; set; }
        public string? SupplierName { get; set; }
        public int LeadTime { get; set; }
        public string? Issuer { get; set; }
        public DateTime? SendPoDate { get; set; }
        public string? Follower { get; set; }
        public DateTime? ConfirmedDeliveryDate { get; set; }
        public string? ImpactToProduction { get; set; } // "Yes" hoặc "No"
        public string? Status { get; set; } // "Pending" hoặc "Received"
    }
    public class PoDetailViewModel
    {
        public int PO_Detail_Id { get; set; }
        public string? Ngayyc { get; set; }
        public string? Ngayycgiao { get; set; }
        public string? SoPO { get; set; }
        public string? Tentiengviet { get; set; }
        public string? Mahang { get; set; }
        public double Soluong { get; set; }
        public string? Donvi { get; set; }
        public string? Nhacungcap { get; set; }
        public string? DNphathanhpo { get; set; }
        public string? ngayguiPO { get; set; } 
        public string? DNphongban { get; set; }
        public string? ngaynccxngiao { get; set; }
        public string? lichgiao { get; set; }
        public string? anhuongsx { get; set; }
        public string? trangthai { get; set; }
        public string? Danhmuc { get; set; }
        public string? LuongvekhoKhonhap { get; set; }
    }
    public class ManagerDeliveryController : Controller
    {

        public IActionResult ManageDelivery(int page = 1)
        {
       
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            string query = $@"SELECT a.*, Dealine FROM [COST_MANAGEMENT].[dbo].[PO] as a 
                      LEFT JOIN REQUEST as b ON a.Code_Request = b.Code_Request 
                      WHERE Ngayphathanh >= '2026-06-01' ORDER BY Ngayphathanh DESC";

            var lst = sql.GET_DATA_FROM_SQL_TEST(query);
            List<PoDetailViewModel> listPo = new List<PoDetailViewModel>();

            for (int i = 0; i < lst.Rows.Count; i++)
            {
                PoDetailViewModel po = new PoDetailViewModel();

                po.PO_Detail_Id = int.Parse(lst.Rows[i]["PO_Detail_Id"].ToString()!);
                po.Ngayyc = lst.Rows[i]["Ngaytao"].ToString()!.Split(' ')[0];
                po.Ngayycgiao = lst.Rows[i]["Ngaygiaohangdukien"].ToString()!.Split(' ')[0];
                po.SoPO = lst.Rows[i]["SoPO"].ToString();
                po.Tentiengviet = lst.Rows[i]["Tentiengviet"].ToString();
                po.Mahang = lst.Rows[i]["Mahang"].ToString();
                po.Soluong = double.Parse(lst.Rows[i]["Soluong"].ToString()!);
                po.Donvi = lst.Rows[i]["Dovi"].ToString();
                po.Nhacungcap = lst.Rows[i]["TenNCC"].ToString();
                po.DNphathanhpo = lst.Rows[i]["Nguoilamdon"].ToString()?.ToLower();          
                po.DNphongban = lst.Rows[i]["Nguoixacnhan"].ToString();

                po.ngayguiPO = lst.Rows[i]["Ngay_gui_PO"] != null ? lst.Rows[i]["Ngay_gui_PO"].ToString()!.Split(' ')[0] : "";
                po.ngaynccxngiao = "";
                po.lichgiao = "";
                po.anhuongsx = "";
                po.trangthai = "";
                po.LuongvekhoKhonhap = lst.Rows[i]["LuongvekhoKhonhap"].ToString();
                po.Danhmuc = lst.Rows[i]["Danhmuc"].ToString();

                listPo.Add(po);
            }
            // ================= LOGIC PHÂN TRANG CHUẨN 500 BẢN GHI TỪ ĐẦU =================
            int pageSize = 500;
            int totalRecords = listPo.Count; // Đếm lại tổng số bản ghi sau khi đã lọc trạng thái
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Lấy chính xác 500 bản ghi của trang hiện tại
            var pagedList = listPo.OrderByDescending(x => x.PO_Detail_Id)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();

            // Truyền trạng thái bộ lọc và phân trang sang View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;       
            TempData["Tongsoluong"] = totalRecords;

            return View(pagedList);
        }
    }
}

