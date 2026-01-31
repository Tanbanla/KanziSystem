using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class OUT_INPUT_ACCOUNT
{
    public int Id { get; set; }

    public string? Code_Request { get; set; }

    public string? Cost_Center { get; set; }

    public DateOnly? Request_Date { get; set; }

    public double? Total { get; set; }

    public string? Declaration { get; set; }

    public string? Note { get; set; }

    public string? Account_Code { get; set; }

    public string? Account_Name { get; set; }

    public string? Loai { get; set; }

    public string? UserNhap { get; set; }

    public string? ThoigianNhap { get; set; }

    public bool? Tinhchiphichi { get; set; }
}
