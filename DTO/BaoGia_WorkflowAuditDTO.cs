using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class BaoGia_WorkflowAuditDTO
{
    public long AuditID { get; set; }

    public int? WorkflowID { get; set; }

    public int? WorkflowStepID { get; set; }

    public string ActionCode { get; set; } = null!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string ChangedBy { get; set; } = null!;

}
