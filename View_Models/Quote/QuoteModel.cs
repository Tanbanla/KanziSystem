using PRJ_WAREHOUSE_BIVN.DTO;

namespace PRJ_WAREHOUSE_BIVN.View_Models.Quote
{
    public class QuoteModel
    {
        // danh sach phong ban
        public List<TM_SECTIONDTO> DanhSachPhongBan { get; set; } = new List<TM_SECTIONDTO>();
        // Danh sach nhom vi tri
        public List<DEPARTMENTDTO> DanhSachNhomViTri { get; set; } = new List<DEPARTMENTDTO>();
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
        // Danh sách chủng loại hàng theo đơn
        public List<string> listCategory { get; set; }
        // Danh sách nhà cung cấp theo đơn 
        public List<dynamic> listNcc { get; set; }
        // Danh sách mặt hàng theo đơn
        public List<string> listMaterial { get; set; }
        // Role của người dùng
        public string Role { get; set; } = string.Empty;

        // Người thao tác 
        public string? NguoiThaoTac { get; set; }
        // Mã đơn hiện tại
        public string? MaDonHienTai { get; set; }
        // Danh sách báo giá gom nhóm
        public List<dynamic> DanhSachBaoGiaGomNhom { get; set; } = new List<dynamic>();
        // Current request for detail page
        public List<BaoGia_Request_of_QuotationDTO>? CurrentRequest { get; set; }

        // list approvel
        public List<BaoGia_Master_Approver_Send_MailDTO> ListApprovel { get; set; } = new List<BaoGia_Master_Approver_Send_MailDTO>();
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
        public string? ChungLoai { get; set; }

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
        public string? status { get; set; }
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
    // Send mail model
    public class SendMailModel
    {
        public string? To { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public List<string>? Attachments { get; set; }
    }
    // Send Mail
    public class InsertInputQuoteModel
    {
        public string MaDon { get; set; } = string.Empty;
        public List<dynamic> baoGiaDetail { get; set; } = new List<dynamic>();
    }
    // Search Quotation_Results
    public class SearchQuotationResultsModel
    {
        public string? MaDon { get; set; }
        public string? MaNcc { get; set; }
        public string? MaVatTu { get; set; }
        public string? Section { get; set; }
        public string? Status { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
    // Search Approval
    public class SearchApprovalModel
    {
        public int? Step { get; set; }
        public string? SectionCost { get; set; }
    }
    // Save Quotiation Results
    public class SaveQuotationResultsModel
    {
        public string UserApproverNext { get; set; } = string.Empty;
        public List<BaoGia_Detail_of_QuotationDTO> listPick { get; set; } = new List<BaoGia_Detail_of_QuotationDTO>();
    }
    // Approval select
    public class ApprovalSelectModel
    {
        public string maDon { get; set; } = string.Empty;
        public string UserApproverNext { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
    public class ConfirmApproverModel
    {
        public List<ApproverDTO> listCofirm { get; set; }
        public string UserApproverNext { get; set; } = string.Empty;
    }
    // input file chon ncc
    public class ImportPickSupplier
    {
        public IFormFile fileSend { get; set; }
        public string userNextApproval { get; set; }
    }
    // history result update
    public class UpdateHistoryResult
    {
        public string? sectionCode { get; set; }

        public List<int>? listUpdate { get; set; }
    }
}
