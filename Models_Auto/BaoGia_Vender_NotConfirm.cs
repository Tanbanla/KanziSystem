using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_Vender_NotConfirm
{
    public int ID { get; set; }

    public string CHR_MaNcc { get; set; } = null!;

    public string? CHR_Status { get; set; }

    public DateTime? created_at { get; set; }
}
