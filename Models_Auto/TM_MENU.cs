using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_MENU
{
    public string CHR_CODE_MENU { get; set; } = null!;

    public string? NVCHR_MENU { get; set; }

    public string? Loai { get; set; }

    public virtual ICollection<TM_AUTHORITY_MENU> TM_AUTHORITY_MENUs { get; set; } = new List<TM_AUTHORITY_MENU>();
}
