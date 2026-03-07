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

    public string? CHR_MAIL_NGUOIYEUCAU { get; set; }

    public string? CHR_MAIL_NGUOITHAMTRA { get; set; }

    public string? CHR_MAIL_NGUOIPHEDUYET { get; set; }

    public string? CHR_TEN_NGUOIYEUCAU { get; set; }

    public string? CHR_TEN_NGUOITHAMTRA { get; set; }

    public string? CHR_TEN_NGUOIPHEDUYET { get; set; }

    public int? CONFIRM_NGUOIYEUCAU { get; set; }

    public DateTime? DTM_NGUOIYEUCAU { get; set; }

    public string? CHR_ADID_XUATKHO { get; set; }

    public int? CONFIRM_XUATKHO { get; set; }

    public string? CHR_MAIL_XUATKHO { get; set; }

    public DateTime? DTM_XUATKHO { get; set; }

    public string? CHR_TEN_XUATKHO { get; set; }

    public string? CHR_TEN_XACNHAN { get; set; }

    public string? CHR_MAIL_XACNHAN { get; set; }
}
