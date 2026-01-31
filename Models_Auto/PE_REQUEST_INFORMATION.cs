using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class PE_REQUEST_INFORMATION
{
    public int ID { get; set; }

    public string NCHR_REQUEST_CODE { get; set; } = null!;

    public string NCHR_EMPLOYEE_ADID { get; set; } = null!;

    public string NCHR_MATERIAL_CODE { get; set; } = null!;

    public double QTY_NEED { get; set; }

    public string NCHR_UNIT { get; set; } = null!;

    public string NCHR_WAREHOUSE_NAME { get; set; } = null!;

    public DateTime DTM_UPDATE { get; set; }

    public string NCHAR_USER { get; set; } = null!;
}
