using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class REQUEST_DETAIL
{
    public int Id_RequestDetail { get; set; }

    public string Code_Request { get; set; } = null!;

    public int? Id_Request { get; set; }

    public string? Material_Code { get; set; }

    public string? Material_Name { get; set; }

    public string? Material_Name_EN { get; set; }

    public string? Material_Name_ENJP { get; set; }

    public string? Account_Code { get; set; }

    public string? Account_Name { get; set; }

    public string? Unit { get; set; }

    public string? Unit_Real { get; set; }

    public double? Amount { get; set; }

    public double? Price { get; set; }

    public double? Total_exchange { get; set; }

    public double? Rate { get; set; }

    public string? Currency { get; set; }

    public double? Total { get; set; }

    public double? Amount_Real { get; set; }

    public double? Price_Real { get; set; }

    public double? VAT { get; set; }

    public double? Total_exchange_real { get; set; }

    public double? Rate_Real { get; set; }

    public string? Currency_Real { get; set; }

    public double? Total_Real { get; set; }

    public DateOnly? Dealine_Real { get; set; }

    public string? Poisition { get; set; }

    public string? Aim { get; set; }

    public string? Brand { get; set; }

    public string? Guarantee { get; set; }

    public string? Status { get; set; }

    public DateTime? Last_Update { get; set; }

    public string? User_Update { get; set; }

    public string? PO { get; set; }

    public string? Unit_Note { get; set; }

    public string? Phongchiuchiphi { get; set; }

    public string? Vitri { get; set; }

    public string? Id_LichsuXuat { get; set; }

    public string? Kho { get; set; }

    public string? Catagory1 { get; set; }

    public string? Catagory2 { get; set; }

    public string? Catagory3 { get; set; }

    public string? Register { get; set; }

    public string? Good_Code { get; set; }

    public DateOnly? ExpectedDeliveryDate { get; set; }

    public string? Quotation { get; set; }

    public string? MaHangTem { get; set; }

    public bool? V4Mua1Lan { get; set; }

    public double? V4SoLuongCan { get; set; }

    public string? V4DonViCan { get; set; }

    public double? LatestUnitPrice { get; set; }

    public double? CheapestUnitPrice { get; set; }

    public double? VenderAPurchasingUnitPrice { get; set; }

    public double? VenderAPurchasingQty { get; set; }

    public string? VenderAPurchasingUnit { get; set; }

    public double? VenderAComparingUnitPrice { get; set; }

    public double? VenderAExchangeQty { get; set; }

    public string? VenderAComparingUnit { get; set; }

    public double? VenderAVat { get; set; }

    public bool? VenderAChoice { get; set; }

    public int? VenderAQuotationFile { get; set; }

    public double? VenderBPurchasingUnitPrice { get; set; }

    public double? VenderBPurchasingQty { get; set; }

    public string? VenderBPurchasingUnit { get; set; }

    public double? VenderBComparingUnitPrice { get; set; }

    public double? VenderBExchangeQty { get; set; }

    public string? VenderBComparingUnit { get; set; }

    public double? VenderBVat { get; set; }

    public bool? VenderBChoice { get; set; }

    public int? VenderBQuotationFile { get; set; }

    public double? VenderCPurchasingUnitPrice { get; set; }

    public double? VenderCPurchasingQty { get; set; }

    public string? VenderCPurchasingUnit { get; set; }

    public double? VenderCComparingUnitPrice { get; set; }

    public double? VenderCExchangeQty { get; set; }

    public string? VenderCComparingUnit { get; set; }

    public double? VenderCVat { get; set; }

    public bool? VenderCChoice { get; set; }

    public int? VenderCQuotationFile { get; set; }

    public double? VenderDPurchasingUnitPrice { get; set; }

    public double? VenderDPurchasingQty { get; set; }

    public string? VenderDPurchasingUnit { get; set; }

    public double? VenderDComparingUnitPrice { get; set; }

    public double? VenderDExchangeQty { get; set; }

    public string? VenderDComparingUnit { get; set; }

    public double? VenderDVat { get; set; }

    public bool? VenderDChoice { get; set; }

    public int? VenderDQuotationFile { get; set; }

    public double? VenderEPurchasingUnitPrice { get; set; }

    public double? VenderEPurchasingQty { get; set; }

    public string? VenderEPurchasingUnit { get; set; }

    public double? VenderEComparingUnitPrice { get; set; }

    public double? VenderEExchangeQty { get; set; }

    public string? VenderEComparingUnit { get; set; }

    public double? VenderEVat { get; set; }

    public bool? VenderEChoice { get; set; }

    public int? VenderEQuotationFile { get; set; }

    public int? OtherFile { get; set; }

    public double? UnitPriceforBudget { get; set; }

    public double? AmountForBudget { get; set; }

    public int? RowIndex { get; set; }

    public string? Service_Goods { get; set; }

    public string? MethodofShip { get; set; }

    public string? PaymentTerm { get; set; }

    public string? DeliveryTerm { get; set; }

    public string? CostElement { get; set; }

    public string? VCHR_NCC_BAOGIA { get; set; }

    public string? NCHR_LienLac_SHIP { get; set; }

    public string? NCHR_GhiChu_1 { get; set; }

    public string? NCHR_GhiChu_2 { get; set; }

    public DateOnly? DTM_NCC_XacNhan { get; set; }

    public string? CHR_USER_UPDATE_MOLD { get; set; }

    public DateTime? DTM_UPDATE_MOLD { get; set; }
}
