using Microsoft.AspNetCore.Mvc;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class PoViewModel
    {
        public int Id { get; set; } // Số thứ tự hoặc ID
        public DateTime ReqDate { get; set; }
        public DateTime DeliveryReqDate { get; set; }
        public string PoNumber { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public string SupplierName { get; set; }
        public int LeadTime { get; set; }
        public string Issuer { get; set; }
        public DateTime? SendPoDate { get; set; }
        public string Follower { get; set; }
        public DateTime? ConfirmedDeliveryDate { get; set; }
        public string ImpactToProduction { get; set; } // "Yes" hoặc "No"
        public string Status { get; set; } // "Pending" hoặc "Received"
    }
    public class ManagerDeliveryController : Controller
    {
        public async Task<IActionResult> ManageDelivery()
        {
            // Lấy dữ liệu từ DB (Ví dụ sử dụng Entity Framework)
            // var data = await _context.PurchaseOrders.Select(po => new PoViewModel { ... }).ToListAsync();

            // Giả lập dữ liệu
            var data = new List<PoViewModel>
            {
                new PoViewModel { Id = 153, PoNumber = "2603-0008", Status = "Received", ImpactToProduction = "Yes" },
                new PoViewModel { Id = 154, PoNumber = "2603-0009", Status = "Pending", ImpactToProduction = "No" },
                new PoViewModel { Id = 155, PoNumber = "2603-0010", Status = "Received", ImpactToProduction = "No" }
            };

            return View(data);
        }
    }
}

