using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class MATERIAL
{
    public int Id_Material { get; set; }

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

    public string? Group_Code { get; set; }

    public string? GoodKind { get; set; }

    public string? Category_VN { get; set; }

    public string? Category_JP { get; set; }

    public string? Shape { get; set; }

    public string? Material { get; set; }

    public string? Composition { get; set; }

    public string? Dimension { get; set; }

    public string? UsedFor { get; set; }

    public string? Purpose { get; set; }

    public string? Category_EN { get; set; }
    public string? Code_Suppiler { get; set; }
}
