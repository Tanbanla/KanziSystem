using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class PE_REQUEST_CONFIRM
{
    public int ID { get; set; }

    public int ID_REQUEST { get; set; }

    public string CHR_ADID_NGUOIYEUCAU { get; set; } = null!;

    public string CHR_ADID_NGUOITHAMTRA { get; set; } = null!;

    public string CHR_ADID_NGUOIPHEDUYET { get; set; } = null!;

    public string CHR_ADID_XACNHAN { get; set; } = null!;

    public bool? CONFIRM_NGUOITHAMTRA { get; set; }

    public bool? CONFIRM_NGUOIPHEDUYET { get; set; }

    public bool? CONFIRM_XACNHAN { get; set; }

    public DateTime? DTM_XACNHAN { get; set; }

    public DateTime? DTM_NGUOITHAMTRA { get; set; }

    public DateTime? DTM_NGUOIPHEDUYET { get; set; }

    public int INT_STEP { get; set; }
}
