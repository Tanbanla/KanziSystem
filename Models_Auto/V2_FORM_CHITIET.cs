using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class V2_FORM_CHITIET
{
    public int MaDonChiTiet { get; set; }

    public string MaDon { get; set; } = null!;

    public int? Id_Madon { get; set; }

    public string? Diengiai { get; set; }

    public string? ActualEstimate { get; set; }

    public string? Sotaikhoan { get; set; }

    public string? TenTaiKhoan { get; set; }

    public double? SoTienUSD { get; set; }

    public double? SoTienNT { get; set; }

    public string? LoaiTien { get; set; }

    public double? TyGia { get; set; }

    public string? OverseaDomestic { get; set; }

    public string? Ncc { get; set; }

    public string? Kydichvu { get; set; }

    public string? Dieukhoanthanhtoan { get; set; }

    public string? LoaiChiPhi { get; set; }

    public DateOnly? NgayYeuCau { get; set; }

    public DateOnly? ThangTinhChiPhi { get; set; }

    public string? TinhTrang { get; set; }

    public string? Cost_Center { get; set; }

    public string? TenPhong { get; set; }

    public string? LoaiDon { get; set; }

    public DateOnly? Ngaythuchi { get; set; }

    public string? Note { get; set; }

    public string? Phongchiuchiphi { get; set; }

    public virtual V2_FORM MaDonNavigation { get; set; } = null!;
}
