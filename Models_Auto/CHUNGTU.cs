using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class CHUNGTU
{
    public int Id_Chungtu { get; set; }

    public string? LOAI { get; set; }

    public string MaChungtu { get; set; } = null!;

    public string? TenEN { get; set; }

    public string? TenVN { get; set; }

    public string? Version { get; set; }

    public string? Cost_Center { get; set; }
}
