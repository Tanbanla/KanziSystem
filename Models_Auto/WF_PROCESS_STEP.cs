using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class WF_PROCESS_STEP
{
    public int Id_Process_Step { get; set; }

    public string Id_Process { get; set; } = null!;

    public string? Pic { get; set; }

    public string? PicName { get; set; }

    public string? Position { get; set; }

    public string? Role { get; set; }

    public string? SubPic { get; set; }

    public string? SubPicName { get; set; }

    public string? SubPosition { get; set; }

    public string? SubRole { get; set; }

    public int? Step { get; set; }

    public DateTime? Process_Date { get; set; }

    public string? Process_User { get; set; }

    public string? Process_UserName { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public virtual WF_PROCESS Id_ProcessNavigation { get; set; } = null!;
}
