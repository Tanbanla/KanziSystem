using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class DEPARTMENTDTO
{
    public int Id_Dept { get; set; }

    public string Cost_Center { get; set; } = null!;

    public string? Name { get; set; }

    public string? Name_Jp { get; set; }

    public string? Cost_Center_Group { get; set; }

    public bool? Active { get; set; }

    public bool? AcceptRequest { get; set; }

    public string? CHR_WAREHOUSE { get; set; }

    public string? CHR_Section_Code { get; set; }
}
