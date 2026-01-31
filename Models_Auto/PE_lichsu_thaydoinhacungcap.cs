using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class PE_lichsu_thaydoinhacungcap
{
    public int ID { get; set; }

    public string NCHR_CHANGE_VENDER { get; set; } = null!;

    public DateTime DTM_EDITED { get; set; }

    public string CHR_USER_EDIT { get; set; } = null!;
}
