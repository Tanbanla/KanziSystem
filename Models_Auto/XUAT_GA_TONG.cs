using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class XUAT_GA_TONG
{
    public string MaNguyenLieu { get; set; } = null!;

    public double? Soluong { get; set; }

    public string? Unit { get; set; }

    public string? MaCost { get; set; }

    public double? Price { get; set; }

    public string? Kho { get; set; }

    public string? Khoi { get; set; }

    public DateOnly? Ngaynhaokho { get; set; }
}
