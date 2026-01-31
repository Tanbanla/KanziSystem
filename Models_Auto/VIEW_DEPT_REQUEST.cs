using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class VIEW_DEPT_REQUEST
{
    public int Id_Dept { get; set; }

    public string Cost_Center { get; set; } = null!;

    public string? Name { get; set; }

    public string? Name_Jp { get; set; }

    public string? Cost_Center_Group { get; set; }

    public bool? Id_RequestDetail { get; set; }

    public int Id_Request { get; set; }

    public string Material_Code { get; set; } = null!;

    public int Material_Name { get; set; }

    public string? Material_Name_ENJP { get; set; }

    public string? Account_Code { get; set; }

    public string? Account_Name { get; set; }

    public string? Unit { get; set; }

    public string? Unit_Real { get; set; }

    public string? Amount { get; set; }

    public string? Price { get; set; }

    public double? Total_exchange { get; set; }

    public double? Rate { get; set; }

    public string? Total { get; set; }

    public string? Amount_Real { get; set; }

    public double? Price_Real { get; set; }

    public double? Total_exchange_real { get; set; }

    public double? Rate_Real { get; set; }

    public string? Total_Real { get; set; }

    public string? Currency { get; set; }

    public double? Poisition { get; set; }

    public string? Aim { get; set; }

    public string? Brand { get; set; }

    public string? Guarantee { get; set; }

    public string? Status { get; set; }

    public string? Last_Update { get; set; }

    public string? User_Update { get; set; }

    public DateTime? PO { get; set; }

    public string? Dealine_Real { get; set; }

    public string? Unit_Note { get; set; }

    public DateOnly? Expr1 { get; set; }

    public string? Expr2 { get; set; }
}
