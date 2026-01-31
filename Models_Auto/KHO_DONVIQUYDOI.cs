using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class KHO_DONVIQUYDOI
{
    public int Id_Quydoi { get; set; }

    public string MaNguyenLieu { get; set; } = null!;

    public string DonviRequest { get; set; } = null!;

    public string DonviPO { get; set; } = null!;

    public double Soluongquydoi { get; set; }

    public string? Khoi { get; set; }
}
