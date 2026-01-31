using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class KHO_CHITIET
{
    public int Id_Kho { get; set; }

    public string MaNguyenLieu { get; set; } = null!;

    public string? Material_Name_VN { get; set; }

    public string? Material_Name_EN { get; set; }

    public string? Material_Name_JP { get; set; }

    public double? Hientai { get; set; }

    public string? Unit { get; set; }

    public string? Unit_Note { get; set; }

    public double? ToiThieu { get; set; }

    public double? ToiDa { get; set; }

    public string Group_Code { get; set; } = null!;

    public string Kho { get; set; } = null!;
}
