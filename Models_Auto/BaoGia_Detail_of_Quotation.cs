using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_Detail_of_Quotation
{
    public int ID { get; set; }

    public int ID_RequestQuote { get; set; }

    public string CHR_CodeNCC { get; set; } = null!;

    public string NVCHR_NameNCC { get; set; } = null!;

    public string? CHR_MaHangNCC { get; set; }

    public string? NVCHR_TenHangHQ { get; set; }

    public double? FL_USD { get; set; }

    public double? FL_VND { get; set; }

    public DateTime? DTM_EndDate { get; set; }

    public string? NVCHR_MOQ { get; set; }

    public string? DTM_LeadTime { get; set; }

    public DateTime? DTM_ShipTime { get; set; }

    public string? NVCHR_Packing { get; set; }

    public bool? BIT_Commit { get; set; }

    public string? NVCHR_Note { get; set; }

    public string? NVCHR_File { get; set; }

    public DateTime DTM_CreateDate { get; set; }

    public string CHR_CreateBy { get; set; } = null!;

    public DateTime? DTM_UpdateDate { get; set; }

    public string? CHR_UpdateBy { get; set; }

    public double? FL_Sum { get; set; }

    public bool? BIT_Select { get; set; }

    public string? NVCHR_ReasonPick { get; set; }

    public string? CHR_Status { get; set; }

    public int? INT_NumberEdit { get; set; }

    public string? NVCHR_dataOld { get; set; }

    public string? NVCHR_dataNew { get; set; }

    public double? FL_ExchangeRate { get; set; }

    public double? FL_TaxRate { get; set; }

    public double? FL_TaxAmount { get; set; }

    public double? FL_TotalAfterTax { get; set; }

    public string? NVCHR_PaymentTerm { get; set; }

    public string? NVCHR_Warranty { get; set; }

    public string? NVCHR_DeliveryTerm { get; set; }

    public string? VCHR_Rohs { get; set; }

    public string? VCHR_COCQ { get; set; }


    public string? VCHR_MSDS { get; set; }

    public string? VCHR_AnToan { get; set; }

    public string? VCHR_CamKet { get; set; }

    public string? NVCHR_NhaSanXuat { get; set; }
    public string? NVCHR_DonVi { get; set; }
    public string? CHR_NameEN { get; set; }
    public int? INT_SoLuong { get; set; }
}
