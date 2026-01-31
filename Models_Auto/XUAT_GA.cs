using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class XUAT_GA
{
    public DateOnly? Ngaynhaokho { get; set; }

    public string MaNguyenLieu { get; set; } = null!;

    public string? Material_Name_JP { get; set; }

    public string? Material_Name_VN { get; set; }

    public double? Soluong { get; set; }

    public string? Unit { get; set; }

    public string? MaCost { get; set; }

    public string? MaChuyen { get; set; }

    public double? Price { get; set; }

    public string? Kho { get; set; }

    public string? Khoi { get; set; }

    public string? Sotaikhoan { get; set; }
}
