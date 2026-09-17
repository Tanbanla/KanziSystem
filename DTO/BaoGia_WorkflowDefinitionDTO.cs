using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class BaoGia_WorkflowDefinitionDTO
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
}
