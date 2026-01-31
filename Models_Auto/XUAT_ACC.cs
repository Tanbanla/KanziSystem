using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class XUAT_ACC
{
    public string? MaChuyen { get; set; }

    public string? MaPhong { get; set; }

    public string? MaMay { get; set; }

    public string? TenChuyen { get; set; }

    public string MaNguyenLieu { get; set; } = null!;

    public double? Soluong { get; set; }

    public DateOnly? Ngaynhaokho { get; set; }

    public string? Kho { get; set; }

    public string? Khoi { get; set; }

    public string? Phong { get; set; }

    public string? Unit { get; set; }
}
