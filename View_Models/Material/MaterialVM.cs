using PRJ_WAREHOUSE_BIVN.DTO;

namespace PRJ_WAREHOUSE_BIVN.View_Models.Material
{
    public class MaterialVM
    {
        public List<DEPARTMENTDTO> vitris { get; set; } = new List<DEPARTMENTDTO>();

        public List<dynamic> confirmedCodes { get; set; } = new List<dynamic>();

        // danh sach vat tu
        public List<MATERIALDTO> DanhSachVatTu { get; set; } = new List<MATERIALDTO>();
        // Danh sach mã đơn 
        public List<string> DanhSachMaDon { get; set; } = new List<string>();

        // Request/Response DTOs for ConfirmName APIs
        public class ConfirmNameSearchRequest
        {
            public string? TenHang { get; set; }
            public string? SoDon { get; set; }
            public string? TrangThai { get; set; }
            public string? Section { get; set; }
            public int pageIndex { get; set; } = 1;
            public int pageSize { get; set; } = 20;
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

        public class MaterialCreate
        {
            public string? MaterialCode { get; set; }  // Required
            public string? MaterialNameVN { get; set; }  // Required
            public string? MaterialNameEN { get; set; }
            public string? MaterialNameJP { get; set; }
            public string? AccountCode { get; set; }
            public string? AccountNameEN { get; set; }
            public string? AccountNameVN { get; set; }
            public string? Unit { get; set; }
            public string? UnitNote { get; set; }
            public decimal? Price { get; set; }
            public string? Currency { get; set; }  
            public string? GroupCode { get; set; }
            public string? GoodKind { get; set; }
        }
        // search material view
        public class SearchMaterialVM
        {
            public string? MaterialCode { get; set; }
            public string? MaterialName { get; set; }
            public string? MaterialCatergory { get; set; }
            public string? MaterialGroup { get; set; }
            public int? pageIndex { get; set; } = 1;
            public int? pageSize { get; set; } = 50;
        }
    }
}
