using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class PE_lichsu_thaydoiuser
{
    public int ID { get; set; }

    public string VCHR_DATA { get; set; } = null!;

    public DateTime DTM_SAVED { get; set; }

    public string CHR_USER_CHANGE { get; set; } = null!;
}
