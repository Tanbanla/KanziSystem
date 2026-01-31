using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class V3_EMAIL
{
    public int Id_Email { get; set; }

    public string CHR_USERID { get; set; } = null!;

    public string? CHR_CRT_USERID { get; set; }

    public string? FULLNAME { get; set; }

    public string? Department { get; set; }
}
