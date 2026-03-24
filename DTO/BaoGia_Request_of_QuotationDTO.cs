using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class BaoGia_Request_of_QuotationDTO
{
    public int ID { get; set; }

    public string? CHR_MaDon { get; set; }

    public string? CHR_MaThietBi { get; set; }

    public string? CHR_Phanloai { get; set; }

    public string? CHR_MaHangNoiBo { get; set; }

    public string? CHR_MaHangNCC { get; set; }

    public string? NVCHR_NameVN { get; set; }

    public string? CHR_NameEN { get; set; }

    public double? INT_SoLuong { get; set; }

    public string? NVCHR_DonVi { get; set; }

    public string? NVCHR_ChungLoai { get; set; }

    public string? NVCHR_HinhDang { get; set; }

    public string? NVCHR_ChatLieu { get; set; }

    public string? NVCHR_ThanhPhan { get; set; }

    public string? NVCHR_KichThuoc { get; set; }

    public string? NVCHR_DongMay { get; set; }

    public string? NVCHR_TinhNang { get; set; }

    public string? NVCHR_Rohs { get; set; }

    public string? NVCHR_COCQ { get; set; }

    public string? NVCHR_MSDS { get; set; }

    public string? NVCHR_AnToan { get; set; }

    public string? NVCHR_FileThietKe { get; set; }

    public string? NVCHR_NhaSanXuat { get; set; }

    public string? CHR_MaNCC { get; set; }

    public string? NVCHR_TenNCC { get; set; }

    public bool? BIT_LayBaoGia { get; set; }

    public string? NVCHR_LyDo { get; set; }

    public DateTime? DTM_NgayMuonNhan { get; set; }

    public DateTime? DTM_KyHan { get; set; }

    public string? CHR_Gap { get; set; }

    public string? CHR_SectionCode { get; set; }

    public string? CHR_SectionName { get; set; }

    public string CHR_CreateBy { get; set; } = null!;

    public DateTime DTM_CreateDate { get; set; }

    public int? ID_StepBaoGia { get; set; }

    public string? ID_Status { get; set; }

    public int? INT_SoLanUpdate { get; set; }

    public DateTime? DTM_UpdateLater { get; set; }

    public DateTime? DTM_Deadline { get; set; }

    public bool? BIT_IsTemplate { get; set; }

    public string? CHR_UserApproval { get; set; }
}
