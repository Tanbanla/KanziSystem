using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class PO_Result_ThueNhaThau
{
    public DateTime? DTM_DATE_INSTOCK { get; set; }

    public string? CHR_STATUS_IN_OUT { get; set; }

    public string? CHR_STATUS_UPDATE { get; set; }

    public string? CHR_PURPOSE { get; set; }

    public string? CHR_PO_ER { get; set; }

    public string? ID_PO_ER_DETAIL { get; set; }

    public string? PO { get; set; }

    public int? INT_QTY_PO_10 { get; set; }

    public decimal? TongUSD_10 { get; set; }
}
