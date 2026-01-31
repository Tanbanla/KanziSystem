using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class KHO_KIEMKE
{
    public int Id_Kiemke { get; set; }

    public string MaNguyenLieu { get; set; } = null!;

    public double? Soluong { get; set; }

    public DateOnly Thang { get; set; }

    public string Group_Code { get; set; } = null!;

    public string Kho { get; set; } = null!;

    public DateTime? NgayCapnhat { get; set; }

    public string? UserCapnhat { get; set; }
}
