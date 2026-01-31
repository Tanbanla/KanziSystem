using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class KHO
{
    public int Id_Kho { get; set; }

    public string MaNguyenLieu { get; set; } = null!;

    public double? Hientai { get; set; }

    public double? ToiThieu { get; set; }

    public double? ToiDa { get; set; }

    public string Group_Code { get; set; } = null!;

    public string Kho1 { get; set; } = null!;

    public string? nvchr_note { get; set; }

    public string? NVCHR_COST { get; set; }

    public DateTime DTM_UPDATE { get; set; }

    public bool IS_SAVE_WH { get; set; }

    public double QTY_NEW { get; set; }

    public double QTY_RE_IMPORT { get; set; }
}
