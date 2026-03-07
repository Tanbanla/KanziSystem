using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_Step
{
    public int ID { get; set; }

    public string CHR_StepName { get; set; } = null!;

    public string CHR_CreateBy { get; set; } = null!;

    public DateTime DTM_CreateDate { get; set; }

    public string? CHR_Note { get; set; }

    public string? CHR_Status { get; set; }

    public int? INT_StepNumber { get; set; }

    public string? CHR_StepNameEN { get; set; }

    public string? CHR_StepNameJP { get; set; }
}
