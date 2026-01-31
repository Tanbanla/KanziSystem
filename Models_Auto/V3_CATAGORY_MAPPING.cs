using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class V3_CATAGORY_MAPPING
{
    public int Id_Catagorymap { get; set; }

    public int Id_Catagory { get; set; }

    public string Material_Code { get; set; } = null!;
}
