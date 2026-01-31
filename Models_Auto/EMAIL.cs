using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class EMAIL
{
    public int Id { get; set; }

    public string? CHR_USERID { get; set; }

    public string? Cost_Center { get; set; }

    public bool? Chiphi { get; set; }

    public virtual TM_USER? CHR_USER { get; set; }
}
