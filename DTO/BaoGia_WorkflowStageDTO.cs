using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class BaoGia_WorkflowStageDTO
{
    public int StageID { get; set; }

    public string StageCode { get; set; } = null!;

    public string StageName { get; set; } = null!;

    public int StageOrder { get; set; }

    public bool IsActive { get; set; }

    public int StageSteps { get; set; }
}
