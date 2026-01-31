using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class USER
{
    public string CHR_USERID { get; set; } = null!;

    public string? VCHR_PASSWORD { get; set; }

    public string? FULLNAME { get; set; }

    public string? Group_Code { get; set; }

    public string? CHR_CRT_USERID { get; set; }

    public decimal? INT_LOCK { get; set; }
}
