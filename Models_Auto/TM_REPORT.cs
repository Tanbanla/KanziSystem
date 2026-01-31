using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_REPORT
{
    public int INT_MAYEUCAU { get; set; }

    public string? CHR_TENYEUCAU { get; set; }

    public DateTime? DTM_NGAYTAODON { get; set; }

    public string? CHR_ADID_NGUOIYEUCAU { get; set; }

    public string? CHR_LINK_PDF { get; set; }

    public string? CHR_EXCEL { get; set; }

    public string? CHR_GHICHU { get; set; }

    public string? CHR_ADID_QLSC { get; set; }

    public string? CHR_ADID_QLTC { get; set; }

    public string? CHR_ADID_ACC { get; set; }

    public string? CHR_STT_QLSC { get; set; }

    public DateTime? DTM_CHOP_QLSC { get; set; }

    public string? CHR_STT_QLTC { get; set; }

    public DateTime? DTM_CHOP_QLTC { get; set; }

    public string? CHR_STT_ACC { get; set; }

    public DateTime? DTM_CHOP_ACC { get; set; }

    public string? CHR_LYDO_TUCHOI { get; set; }

    public string? CHR_STT_NGUOIYEUCAU { get; set; }
}
