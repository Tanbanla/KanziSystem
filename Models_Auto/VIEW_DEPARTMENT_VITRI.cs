using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class VIEW_DEPARTMENT_VITRI
{
    public int Id_Vitri { get; set; }

    public string? MaCost { get; set; }

    public string? MaPhong { get; set; }

    public string MaChuyen { get; set; } = null!;

    public string? MaMay { get; set; }

    public string? TenChuyen { get; set; }

    public string? Mahangmuctheovitri { get; set; }
}
