using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class Master_RECEIVE_EMAIL_PRICE
{
    public string CHR_EMPLOYEE_ID { get; set; } = null!;

    public string? CHR_EMPLOYEE_NAME { get; set; }

    public string? CHR_EMPLOYEE_ADID { get; set; }

    public string? CHR_SEC_CODE { get; set; }

    public string? CHR_POSTION_GROUP { get; set; }

    public bool? BIT_STATUS { get; set; }

    public string? CHR_USER_CREATE { get; set; }

    public DateTime? DTM_USER_CREATE { get; set; }

    public string? CHR_USER_UPDATE { get; set; }

    public DateTime? DTM_USER_UPDATE { get; set; }
}
