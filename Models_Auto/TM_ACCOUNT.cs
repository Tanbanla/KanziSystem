using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_ACCOUNT
{
    public int Id_Account { get; set; }

    public string Account_Code { get; set; } = null!;

    public string? Account_Name_EN { get; set; }

    public string? Account_Name_VN { get; set; }

    public string? Account_Name_JP { get; set; }

    public string? Note { get; set; }

    public string? LoaiChiPhi { get; set; }

    public bool? Tinhchiphichi { get; set; }
}
