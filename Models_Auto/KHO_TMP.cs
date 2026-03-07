using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class KHO_TMP
{
    public int ID { get; set; }

    public string CHR_CODE_MATERIAL { get; set; } = null!;

    public string CHR_WAREHOUSE { get; set; } = null!;

    public double QUANTITY { get; set; }

    public string CHR_GROUP_CODE { get; set; } = null!;

    public DateTime DTM_TIMEIMPORT { get; set; }
}
