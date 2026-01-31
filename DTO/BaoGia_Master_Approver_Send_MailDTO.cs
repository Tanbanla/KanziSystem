using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class BaoGia_Master_Approver_Send_MailDTO
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
public class GetApproversRequestDTO
{
    public string? SectionCode { get; set; }
    public string? Adid { get; set; }
    public int? IdStep { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 1000;
}