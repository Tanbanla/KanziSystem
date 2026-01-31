using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class CHUYENTIEN_CHITIET
{
    public int Id_Chuyen { get; set; }

    public string MaChuyenTien { get; set; } = null!;

    public string? PhongChuyen { get; set; }

    public DateOnly? ThangChuyen { get; set; }

    public double? SotienChuyen { get; set; }

    public string? LoaiChiPhiChuyen { get; set; }

    public string? SotaikhoanChuyen { get; set; }

    public string? LoaiChuyen { get; set; }

    public int? Dong { get; set; }

    public int? Cot { get; set; }

    public int? NamTaiChinh { get; set; }

    public virtual CHUYENTIEN MaChuyenTienNavigation { get; set; } = null!;
}
