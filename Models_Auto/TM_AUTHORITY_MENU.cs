using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_AUTHORITY_MENU
{
    public string CHR_USERID { get; set; } = null!;

    public string CHR_CODE_MENU { get; set; } = null!;

    public int? INT_NUMBER_POSITION { get; set; }

    public string? CHR_CRT_USERID { get; set; }

    public DateOnly? DTM_CREATE { get; set; }

    public string? CHR_UPD_USERID { get; set; }

    public DateOnly? DTM_UPDATE { get; set; }

    public virtual TM_MENU CHR_CODE_MENUNavigation { get; set; } = null!;

    public virtual TM_USER CHR_USER { get; set; } = null!;
}
