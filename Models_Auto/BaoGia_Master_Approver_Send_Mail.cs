using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_Master_Approver_Send_Mail
{
    public int ID { get; set; }

    public int? ID_BaoGiaStep { get; set; }

    public string? CHR_UserAdid { get; set; }

    public string? CHR_CodeSection { get; set; }

    public string? CHR_NameSection { get; set; }

    public string? NVCHR_UserName { get; set; }

    public string? NVCHR_Position { get; set; }

    public string? NVCHR_StepName { get; set; }

    public string? CHR_CreateBy { get; set; }

    public DateTime? CHR_CreateDate { get; set; }

    public string? CHR_Status { get; set; }

    public string? CHR_UpdateBy { get; set; }

    public DateTime? CHR_UpdateDate { get; set; }
}
