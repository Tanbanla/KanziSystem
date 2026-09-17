using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class BaoGia_WorkflowDefinitionStepDTO
{
    public int WorkflowStepID { get; set; }

    public int WorkflowID { get; set; }

    public int StepID { get; set; }

    public int StepOrder { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsRequired { get; set; }

    public bool AllowSkip { get; set; }

    public bool IsFinalStep { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }
}
