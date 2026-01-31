using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class CHUNGTU_DANHAN
{
    public int Id_Chungtudanhan { get; set; }

    public int? Id_Chungtu { get; set; }

    public string? Sonhan { get; set; }

    public string? LOAI { get; set; }

    public string MaChungtu { get; set; } = null!;

    public string? TenEN { get; set; }

    public string? TenVN { get; set; }

    public string? Version { get; set; }

    public string? Cost_Center { get; set; }

    public string? Tinhtrang { get; set; }

    public string? Nguoitao { get; set; }

    public DateTime? Thogiantao { get; set; }

    public string? Nguoinhan { get; set; }

    public DateTime? Thoigiannhan { get; set; }

    public string? Nguoixoa { get; set; }

    public DateTime? Thoigianxoa { get; set; }

    public string? Phongnop { get; set; }
}
