using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class CATAGORY_MATERIAL_MASTER
{
    public string? Catagory1 { get; set; }

    public string? Catagory2 { get; set; }

    public string? Catagory3 { get; set; }

    public string Material_Code { get; set; } = null!;

    public string? Material_Name_VN { get; set; }

    public string? Material_Name_EN { get; set; }

    public string? Material_Name_JP { get; set; }

    public string? Account_Code { get; set; }

    public string? Account_Name_EN { get; set; }

    public string? Account_Name_VN { get; set; }

    public string? Unit { get; set; }

    public string? Unit_Note { get; set; }

    public double? Price { get; set; }

    public string? Currency { get; set; }

    public string Group_Code { get; set; } = null!;

    public string? GoodKind { get; set; }
}
