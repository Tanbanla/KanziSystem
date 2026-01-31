using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class VIEW_PROCESS_PRIVATE
{
    public string Id_Process { get; set; } = null!;

    public string? Code_Request { get; set; }

    public DateTime? Create_Date { get; set; }

    public string? Create_User { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public DateTime? Process_Date { get; set; }

    public string? Process_User { get; set; }
}
