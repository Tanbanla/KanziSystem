using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_Status
{
    public int ID { get; set; }

    public string NVCHR_TenStatus { get; set; } = null!;

    public string CHR_CreateBy { get; set; } = null!;

    public DateTime DTM_CreateDate { get; set; }

    public string CHR_Flag { get; set; } = null!;

    public string? VCHR_CodeStatus { get; set; }

    public string? CHR_TenStatusJP { get; set; }

    public string? CHR_TenStatusEN { get; set; }
}
