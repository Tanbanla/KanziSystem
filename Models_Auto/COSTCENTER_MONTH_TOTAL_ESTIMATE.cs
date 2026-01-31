using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class COSTCENTER_MONTH_TOTAL_ESTIMATE
{
    public string? Cost_Center { get; set; }

    public string Code_Request { get; set; } = null!;

    public string? Declaration { get; set; }

    public DateOnly? Dealine { get; set; }

    public string? StatusTotal { get; set; }

    public int Id_RequestDetail { get; set; }

    public int? Id_Request { get; set; }

    public string? Material_Code { get; set; }

    public string? Material_Name { get; set; }

    public string? Material_Name_ENJP { get; set; }

    public string? Account_Code { get; set; }

    public string? Account_Name { get; set; }

    public double? Amount { get; set; }

    public double? Amount_Real { get; set; }

    public string? Unit { get; set; }

    public string? Unit_Real { get; set; }

    public double? Price { get; set; }

    public double? Price_Real { get; set; }

    public double? Total { get; set; }

    public double? Total_Real { get; set; }

    public string? Currency { get; set; }

    public string? Poisition { get; set; }

    public string? Aim { get; set; }

    public string? Brand { get; set; }

    public string? Guarantee { get; set; }

    public string? Status { get; set; }

    public DateTime? Last_Update { get; set; }

    public string? User_Update { get; set; }

    public string? PO { get; set; }

    public DateOnly? Dealine_Real { get; set; }

    public string? Kind { get; set; }

    public DateTime? Create_Date { get; set; }

    public double? Rate_Real { get; set; }

    public double? Total_exchange_real { get; set; }

    public string? Group_Code { get; set; }

    public string? Phongchiuchiphi { get; set; }

    public bool? Chophepin { get; set; }

    public bool? Tinhchiphichi { get; set; }

    public string? Vitri { get; set; }

    public string? Id_LichsuXuat { get; set; }

    public string? Material_Name_EN { get; set; }

    public string? Kho { get; set; }

    public double? VAT { get; set; }

    public string? Good_Code { get; set; }
}
