using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_WorkflowStage
{
    public int StageID { get; set; }

    public string StageCode { get; set; } = null!;

    public string StageName { get; set; } = null!;

    public int StageOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<BaoGia_WorkflowStep> BaoGia_WorkflowSteps { get; set; } = new List<BaoGia_WorkflowStep>();
}
