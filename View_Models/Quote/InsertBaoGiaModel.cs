namespace PRJ_WAREHOUSE_BIVN.View_Models.Quote
{
    public class InsertBaoGiaModel
    {
        public string? CHR_CreateBy { get; set; }

        public string? CHR_Gap { get; set; }

        public string? CHR_MaHangNoiBo { get; set; }

        public string? CHR_MaHangNCC { get; set; }

        public string? CHR_MaThietBi { get; set; }

        public string? CHR_NameEN { get; set; }

        public string? CHR_Phanloai { get; set; }

        public string? CHR_SectionCode { get; set; }

        public string? CHR_SectionName { get; set; }

        public DateTime? DTM_Deadline { get; set; }

        public DateTime? DTM_KyHan { get; set; }

        public DateTime? DTM_NgayMuonNhan { get; set; }

        public string? NVCHR_AnToan { get; set; }

        public string? NVCHR_COCQ { get; set; }

        public string? NVCHR_ChatLieu { get; set; }

        public string? NVCHR_ChungLoai { get; set; }

        public string? NVCHR_DonVi { get; set; }

        public string? NVCHR_DongMay { get; set; }

        public string? NVCHR_FileThietKe { get; set; }

        public string? NVCHR_HinhDang { get; set; }

        public string? NVCHR_KichThuoc { get; set; }

        public string? NVCHR_MSDS { get; set; }

        public string? NVCHR_NameVN { get; set; }

        public string? NVCHR_Rohs { get; set; }

        public string? NVCHR_ThanhPhan { get; set; }

        public string? NVCHR_TinhNang { get; set; }

        public string? CHR_UserApproval { get; set; }

        public string? NVCHR_UserRequest { get; set; }

        public int? INT_SoLuong { get; set; }

        public string? NVCHR_ReasonQuotation { get; set; }

        public List<VendorQuoteModel>? Vendors { get; set; } = new List<VendorQuoteModel>();

        public IFormFile? File_ThietKe { get; set; }
        public string? Link_box { get; set; }
        public IFormFile? File_anh { get; set; }


        // cac truong du lieu them
        public string? CHR_LinkImage { get; set; }
        public string? NVCHR_DiaDiemNH { get; set; }
        public string? NVCHR_NguoiNhan { get; set; }
        public string? CHR_SDT { get; set; }


        public string? WfSection { get; set; }
        public string? WfType { get; set; }
    }

    // Vendor
    public class VendorQuoteModel
    {
        public string? MaNcc { get; set; }
        public string? TenNcc { get; set; }
        public string? NhaSanXuat { get; set; }
        public bool BIT_LayBaoGia { get; set; }
        public string? NVCHR_LyDo { get; set; }

    }
}
