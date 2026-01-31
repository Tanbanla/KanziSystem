using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class ACC_REPORT1
{
    public int Id_Report1 { get; set; }

    public string Cost_Center { get; set; } = null!;

    public string Loaihinhtokhai { get; set; } = null!;

    public DateOnly Thang { get; set; }

    public double? Thucte { get; set; }

    public DateTime? Ngaycapnhat { get; set; }

    public string? Usercapnhat { get; set; }
}
