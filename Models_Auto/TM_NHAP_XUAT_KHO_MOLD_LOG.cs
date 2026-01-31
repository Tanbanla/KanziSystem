using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_NHAP_XUAT_KHO_MOLD_LOG
{
    public int ID { get; set; }

    public int? ID_PO_ER_DETAIL { get; set; }

    public string? CHR_PO_ER { get; set; }

    public string? CHR_RQ_CODE { get; set; }

    public string? CHR_GOOD_NAME { get; set; }

    public int? INT_QTY_PO { get; set; }

    public int? INT_QTY_RECEIVED { get; set; }

    public string? CHR_DONVI { get; set; }

    public DateTime? DTM_DATE_INSTOCK { get; set; }

    public string? CHR_USER_INSTOCK { get; set; }

    public string? CHR_STATUS_IN_OUT { get; set; }

    public DateTime? DTM_DATE_EXPORT { get; set; }

    public string? CHR_USER_EXPORT { get; set; }

    public DateTime? DTM_DATE_RECEIVED { get; set; }

    public string? CHR_USERID_RECEIVED { get; set; }

    public string? CHR_USERNAME_RECEIVED { get; set; }

    public string? CHR_PROJECT_CODE { get; set; }

    public string? CHR_GOOD_CODE_BOOK { get; set; }

    public int? INT_PHANLOAI_HANGHOA { get; set; }

    public string? CHR_NOTE { get; set; }

    public string? CHR_STATUS_UPDATE { get; set; }

    /// <summary>
    /// Mục đích xuất: Xuất dumg, xuất sparepart
    /// </summary>
    public string? CHR_STATUS_IN_STOCK { get; set; }

    public string? CHR_KHO { get; set; }

    public DateTime? DTM_DATE_UPDATE { get; set; }
}
