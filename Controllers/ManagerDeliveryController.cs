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
        public DateTime? Ngayyc { get; set; }
        public DateTime? Ngayycgiao { get; set; }
        public string? SoPO { get; set; }
        public string? Tentiengviet { get; set; }
        public string? Mahang { get; set; }
        public decimal Soluong { get; set; }
        public string? Dovi { get; set; }
        public string? Nhacungcap { get; set; }
        public DateTime? TimeGH { get; set; }
        public string? DNphathanhpo { get; set; }
        public DateTime? ngayguiPO { get; set; } 
        public string? DNncc { get; set; }
        public string? ngaynccxngiao { get; set; }
        public string? lichgiao { get; set; }
        public string? anhuongsx { get; set; }
        public string? trangthai { get; set; }
        public string? Danhmuc { get; set; }
    }
    public class ManagerDeliveryController : Controller
    {
        //public async Task<IActionResult> ManageDelivery()
        //{
      
        //    SQL_Connect_DB20 sql = new SQL_Connect_DB20();
        //    string query = $@"SELECT a.*, Dealine FROM [COST_MANAGEMENT].[dbo].[PO] as a left join REQUEST as b on a.Code_Request = b.Code_Request where Ngayphathanh >= '{DateTime.Now.ToString("yyyy-MM-01")}' order by Ngayphathanh desc";

        //    var lst = sql.GET_DATA_FROM_SQL(query);
        //    for(int i = 0; i < lst.Rows.Count; i++)
        //    {
        //        PoDetailViewModel po = new PoDetailViewModel();
        //        po.PO_Detail_Id = int.Parse(lst.Rows[i][""].ToString()!);
        //    }
        //}
    }
}

