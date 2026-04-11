using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class DEPARTMENT_1
{
    public int Id_Dept { get; set; }

    public string Cost_Center { get; set; } = null!;

    public string? Name { get; set; }

    public string? Name_Jp { get; set; }

    public string? Cost_Center_Group { get; set; }

    public bool? Active { get; set; }

    public bool? AcceptRequest { get; set; }

    public string? CHR_WAREHOUSE { get; set; }

    public string? CHR_Section_Code { get; set; }

    public virtual ICollection<ESTIMATE_DEADLINE_CHANGE> ESTIMATE_DEADLINE_CHANGEs { get; set; } = new List<ESTIMATE_DEADLINE_CHANGE>();

    public virtual ICollection<ESTIMATE> ESTIMATEs { get; set; } = new List<ESTIMATE>();

    public virtual ICollection<REQUEST> REQUESTs { get; set; } = new List<REQUEST>();

    public virtual ICollection<USER_DEPT> USER_DEPTs { get; set; } = new List<USER_DEPT>();

    public virtual ICollection<V2_FORM> V2_FORMs { get; set; } = new List<V2_FORM>();
}
