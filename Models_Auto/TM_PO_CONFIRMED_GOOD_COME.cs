using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_PO_CONFIRMED_GOOD_COME
{
    public string CHR_PO { get; set; } = null!;

    public DateTime? DTM_DATE_CONFIRM { get; set; }

    public string? CHR_USER_CONFIRM { get; set; }
}
