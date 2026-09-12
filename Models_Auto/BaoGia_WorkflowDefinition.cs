using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_WorkflowDefinition
{
    public int WorkflowID { get; set; }

    public int RequestTypeID { get; set; }

    public string FlowCode { get; set; } = null!;

    public string WorkflowName { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<BaoGia_WorkflowDefinitionStep> BaoGia_WorkflowDefinitionSteps { get; set; } = new List<BaoGia_WorkflowDefinitionStep>();

    public virtual BaoGia_RequestType RequestType { get; set; } = null!;
}
