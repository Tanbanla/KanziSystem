using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class USER_DEPT
{
    public int Id_User_Dept { get; set; }

    public string CHR_USERID { get; set; } = null!;

    public string Cost_Center { get; set; } = null!;

    public virtual TM_USER CHR_USER { get; set; } = null!;

    public virtual DEPARTMENT_1 Cost_CenterNavigation { get; set; } = null!;
}
