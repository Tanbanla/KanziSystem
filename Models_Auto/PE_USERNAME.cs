using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class PE_USERNAME
{
    public int Id_User { get; set; }

    public string? User_Name { get; set; }

    public string? Mail { get; set; }

    public string? Adid { get; set; }

    public string? Group_Code { get; set; }

    public int? Role { get; set; }
}
