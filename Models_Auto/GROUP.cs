using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class GROUP
{
    public int Id_Group { get; set; }

    public string Group_Code { get; set; } = null!;

    public string? Group_Name { get; set; }

    public bool? Status { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<GROUP_MEMBER> GROUP_MEMBERs { get; set; } = new List<GROUP_MEMBER>();
}
