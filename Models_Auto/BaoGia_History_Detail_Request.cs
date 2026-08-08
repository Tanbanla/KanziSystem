using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_History_Detail_Request
{
    public int ID { get; set; }

    public int ID_RQ_Detail { get; set; }

    public string? NVCHR_dataOld { get; set; }

    public string? NVCHR_dataNew { get; set; }

    public string? CHR_CreateBy { get; set; }

    public DateTime? DTM_CreateBy { get; set; }

    public string? NVCHR_ReasonUpdate { get; set; }
}
