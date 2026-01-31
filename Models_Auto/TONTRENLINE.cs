using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TONTRENLINE
{
    public int Id_TonLine { get; set; }

    public string MaNguyenLieu { get; set; } = null!;

    public double? Soluong { get; set; }

    public DateOnly Thang { get; set; }

    public string Cost { get; set; } = null!;

    public string Vitri { get; set; } = null!;

    public string Nhamay { get; set; } = null!;

    public string Khoi { get; set; } = null!;

    public string? Ghichu { get; set; }

    public DateTime? NgayCapnhat { get; set; }

    public string? UserCapnhat { get; set; }
}
