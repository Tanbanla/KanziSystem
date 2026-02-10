using PRJ_WAREHOUSE_BIVN.DTO;

namespace PRJ_WAREHOUSE_BIVN.View_Models.Quote
{
    public class QuoteModel
    {
        // danh sach phong ban
        public List<TM_SECTIONDTO> DanhSachPhongBan { get; set; } = new List<TM_SECTIONDTO>();
        // Danh sach nhom vi tri
        public List<ACC_NHOMVITRIDTO> DanhSachNhomViTri { get; set; } = new List<ACC_NHOMVITRIDTO>();
        // danh sach vat tu
        public List<MATERIALDTO> DanhSachVatTu { get; set; } = new List<MATERIALDTO>();
        // danh sach nha cung cap
        public List<IM_NCC_NEWDTO> DanhSachNhaCungCap { get; set; } = new List<IM_NCC_NEWDTO>();
        //
        public List<string> DanhSachCategory { get; set; } = new List<string>();
        // Lich su bao gia
        public List<BaoGia_History_Request_of_QuotationDTO> LichSuBaoGia { get; set; } = new List<BaoGia_History_Request_of_QuotationDTO>();
        // Danh sach yeu cau bao gia
        public List<BaoGia_Request_of_QuotationDTO> DanhSachYeuCauBaoGia { get; set; } = new List<BaoGia_Request_of_QuotationDTO>();
        // Danh sach trang thai
        public List<BaoGia_StatusDTO> DanhSachStatus { get; set; } = new List<BaoGia_StatusDTO>();
        // Danh sach mã đơn 
        public List<string> DanhSachMaDon { get; set; } = new List<string>();
        // Người thao tác 
        public string? NguoiThaoTac { get; set; }
        // Danh sách báo giá gom nhóm
        public List<dynamic> DanhSachBaoGiaGomNhom { get; set; } = new List<dynamic>();
    }
    public class SearchBaoGiaViewModel
    {
        public string? MaDon { get; set; }
        public string? MaNcc { get; set; }
        public string? Section { get; set; }
        public string? NguoiYeuCau { get; set; }
        public string? MaHang { get; set; }
        public string? TrangThai { get; set; }
        public int? Step { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? Date { get; set; }

    }
    public class MaterialSearch
    {
        public string? MaHang { get; set; }
        public string? Name { get; set; }
        public string? NhomHang { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
    public class SearchHitory
    {
        public int? idRequestQuote { get; set; }
        public string? soDon { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
    public class SearchInputQuote
    {
        // int? idRequest, string? maDon, string? maVatTu, string? maNcc, string? section, int? PageSize, int? PageIndex
        public int? idRequestQuote { get; set; }
        public string? maDon { get; set; }
        public string? maVatTu { get; set; }
        public string? maNcc { get; set; }
        public string? section { get; set; }
        public DateTime? dayMM { get; set; }
        public int? pageIndex { get; set; }
        public int? pageSize { get; set; }
    }
    public class ThongTinBaoGiaGomNhomModel
    {
        public string? maDon { get; set; }
        public string? section { get; set; }
        public string? maHang { get; set; }
        public int pageIndex { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }
    public class SelectionExportItem
    {
        public string? ID { get; set; }
        public string? MaDon { get; set; }
    }
   // Paramater Export file
   public class AutoRenderFile
    {
        public List<string> selectedItemIds { get; set; } = new List<string>();
        public string sectionCode { get; set; } = string.Empty;
        public string sectionName { get; set; } = string.Empty;
    }
}
