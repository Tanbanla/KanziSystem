using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class ACC_NHOMHANG_MATERIAL
{
    public int Id_Nhomhang { get; set; }

    public string Manhomhang { get; set; } = null!;

    public string Material_Code { get; set; } = null!;
}
