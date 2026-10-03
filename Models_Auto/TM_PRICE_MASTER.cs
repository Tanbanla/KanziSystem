using System;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_PRICE_MASTER
{
    public int ID { get; set; }
    public DateTime DTM_UPLOAD { get; set; }
    public string? CHR_UPLOAD_USERID { get; set; }
    public string? CHR_QUOTATION_REQUEST_NO { get; set; }
    public int? INT_QUOTATION_DETAIL { get; set; }
    public string? CHR_EQUIPMENT_CODE { get; set; }
    public string CHR_INTERNAL_PART_CODE { get; set; } = null!;
    public string? CHR_VENDOR_PART_CODE { get; set; }
    public string? NVCHR_PART_NAME_VN { get; set; }
    public string? NVCHR_PART_NAME_EN { get; set; }
    public string? NVCHR_UNIT { get; set; }
    public string? NVCHR_OTHER_REQUIREMENT { get; set; }
    public string? NVCHR_MAKER_ORIGIN { get; set; }
    public decimal? DEC_QUANTITY { get; set; }
    public string CHR_VENDOR_CODE { get; set; } = null!;
    public string NVCHR_VENDOR_NAME { get; set; } = null!;
    public decimal? DEC_UNIT_PRICE { get; set; }
    public string? CHR_CURRENCY { get; set; }
    public decimal? DEC_UNIT_PRICE_USD { get; set; }
    public int? INT_LEAD_TIME_DAY { get; set; }
    public decimal? DEC_MOQ { get; set; }
    public string? NVCHR_REMARK { get; set; }
    public string? NVCHR_DELIVERY_TERM { get; set; }
    public string? NVCHR_PLACE { get; set; }
    public string? NVCHR_SHIPMENT_METHOD { get; set; }
    public decimal? DEC_VAT_PERCENT { get; set; }
    public string? NVCHR_PAYMENT_TERM { get; set; }
    public DateTime? DTM_PRICE_EFFECTIVE { get; set; }
    public DateTime? DTM_PRICE_EXPIRATION { get; set; }
    public bool BIT_FIX_VENDOR { get; set; }
    public string? NVCHR_ADJUSTMENT_REASON { get; set; }
    public string? NVCHR_QTN_LINK { get; set; }
    public string? NVCHR_QTN_EXCEL_LINK { get; set; }
    public bool BIT_USE_LEAVE_RATE { get; set; }
    public string? CHR_CRT_USERID { get; set; }
    public DateTime DTM_CREATE { get; set; }
    public string? CHR_UPD_USERID { get; set; }
    public DateTime? DTM_UPDATE { get; set; }
}
