using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class GROUP_MEMBER
{
    public int Group_member_Id { get; set; }

    public string Group_Code { get; set; } = null!;

    public string CHR_USERID { get; set; } = null!;

    public virtual TM_USER CHR_USER { get; set; } = null!;

    public virtual GROUP Group_CodeNavigation { get; set; } = null!;
}
