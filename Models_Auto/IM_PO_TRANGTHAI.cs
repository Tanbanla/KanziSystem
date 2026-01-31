using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class IM_PO_TRANGTHAI
{
    public int id_Trangthai { get; set; }

    public string SoPO { get; set; } = null!;

    public bool? XACNHAN_NEED_NONEED { get; set; }

    public DateTime? XACNHAN_NEED_NONEED_NGAY { get; set; }

    public string? XACNHAN_NEED_NONEED_USER { get; set; }

    public bool? NHAPINVOICE { get; set; }

    public DateTime? NHAPINVOICE_NGAY { get; set; }

    public string? NHAPINVOICE_USER { get; set; }

    public bool? NHAPLUONGVE { get; set; }

    public DateTime? NHAPLUONGVE_NGAY { get; set; }

    public string? NHAPLUONGVE_USER { get; set; }

    public bool? NHAPTOKHAI { get; set; }

    public DateTime? NHAPTOKHAI_NGAY { get; set; }

    public string? NHAPTOKHAI_USER { get; set; }

    public bool? NHAPKHO { get; set; }

    public DateTime? NHAPKHO_NGAY { get; set; }

    public string? NHAPKHO_USER { get; set; }
}
