using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_Category
{
    public int ID { get; set; }

    public string NVCHR_Category { get; set; } = null!;

    public DateTime? DTM_CreateBy { get; set; }

    public string? CHR_CreateBy { get; set; }
}
