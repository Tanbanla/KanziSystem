using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_WorkflowAudit
{
    public long AuditID { get; set; }

    public int? WorkflowID { get; set; }

    public int? WorkflowStepID { get; set; }

    public string ActionCode { get; set; } = null!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string ChangedBy { get; set; } = null!;

    public DateTime ChangedDate { get; set; }
}
