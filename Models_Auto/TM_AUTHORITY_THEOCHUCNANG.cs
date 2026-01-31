using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_AUTHORITY_THEOCHUCNANG
{
    public string CHR_CODE_FUNCTION { get; set; } = null!;

    public string? Mieuta { get; set; }

    public virtual ICollection<TM_USER> CHR_USERs { get; set; } = new List<TM_USER>();
}
