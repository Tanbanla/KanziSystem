using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class WF_WORKFOLLOWSTEP
{
    public int Id_Step { get; set; }

    public string Id_WF { get; set; } = null!;

    public string Position { get; set; } = null!;

    public int Step { get; set; }

    public bool? Locked { get; set; }

    public bool? Edit_Quotation { get; set; }

    public bool? Edit_Category { get; set; }

    public bool? View_Rule { get; set; }

    public bool? Approval_Rule { get; set; }

    public bool? Refuse_Rule { get; set; }

    /// <summary>
    /// Được phép trả về những bước nào
    /// </summary>
    public string? Refuse_Step { get; set; }

    /// <summary>
    /// Những  bước nhận được email thông báo nếu Refuse
    /// </summary>
    public string? Reuse_Email { get; set; }

    public DateTime? Create_Date { get; set; }

    public string? Create_User { get; set; }

    public string? note { get; set; }

    public virtual WF_WORKFOLLOWLIST Id_WFNavigation { get; set; } = null!;
}
