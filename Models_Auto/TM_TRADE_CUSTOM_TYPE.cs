using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_TRADE_CUSTOM_TYPE
{
    public string CHR_TRADE_TYPE { get; set; } = null!;

    public string? CHR_TRADE_TYPE_VN { get; set; }

    public string? CHR_TRADE_TYPE_JP { get; set; }

    public string? CHR_USER_CREATE { get; set; }

    public DateTime? DTM_DATE_CREATE { get; set; }

    public string? CHR_USER_UPDATE { get; set; }

    public DateTime? DTM_DATE_UPDATE { get; set; }
}
