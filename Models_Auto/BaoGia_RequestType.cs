using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_RequestType
{
    public int ID { get; set; }

    public string CHR_Code { get; set; } = null!;

    public string NVCHR_Name { get; set; } = null!;

    public virtual ICollection<BaoGia_WorkflowDefinition> BaoGia_WorkflowDefinitions { get; set; } = new List<BaoGia_WorkflowDefinition>();
}
