using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class BaoGia_Confirm_Name_QuotationDTO
{
    public int ID { get; set; }

    public int ID_RequestQuote { get; set; }

    public string? VCHR_UserShip { get; set; }

    public string? VCHR_UserAcc { get; set; }

    public string? VCHR_MaHangNoiBo { get; set; }

    public string? VCHR_TenHaiQuan { get; set; }

    public DateTime? DTM_Send { get; set; }

    public DateTime? DTM_UserShip { get; set; }

    public DateTime? DTM_UserAcc { get; set; }

    public DateTime DTM_CreateDate { get; set; }

    public string VCHR_CreateBy { get; set; } = null!;

    public DateTime? DTM_UpdateDate { get; set; }

    public string? VCHR_UpdateBy { get; set; }

    public string CHR_Status { get; set; } = null!;

    public string? VCHR_UserPUR { get; set; }

    public DateTime? DTM_UserPUR { get; set; }

    public string? NVCHR_LyDo { get; set; }

    public string? NVCHR_Note { get; set; }

    public string? VCHR_TenRecomment { get; set; }
}
