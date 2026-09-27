namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_WorkflowTransition
{
    public int TransitionID { get; set; }
    public int WorkflowID { get; set; }
    public int FromWorkflowStepID { get; set; }
    public int ToWorkflowStepID { get; set; }
    public string ActionCode { get; set; } = null!;
    public string? ConditionExpression { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public virtual BaoGia_WorkflowDefinition Workflow { get; set; } = null!;
    public virtual BaoGia_WorkflowDefinitionStep FromWorkflowStep { get; set; } = null!;
    public virtual BaoGia_WorkflowDefinitionStep ToWorkflowStep { get; set; } = null!;
}
//CREATE TABLE[dbo].[BaoGia_WorkflowTransition]
//    (
//    [TransitionID] INT             IDENTITY(1,1) NOT NULL,
//    [WorkflowID]           INT NULL,
//    [FromWorkflowStepID]   INT NULL,
//    [ToWorkflowStepID]     INT NULL,
//    [ActionCode]           VARCHAR(30)     NOT NULL,
//    [ConditionExpression]  NVARCHAR(2000)  NULL,
//    [CreatedBy] NVARCHAR(100)   NULL,
//    [CreatedDate] DATETIME2(0)    NULL CONSTRAINT[DF_BaoGia_WorkflowTransition_CreatedDate] DEFAULT(SYSDATETIME()),
//    [UpdatedBy] NVARCHAR(100)   NULL,
//    [UpdatedDate] DATETIME2(0)    NULL,

//    CONSTRAINT[PK_BaoGia_WorkflowTransition] PRIMARY KEY CLUSTERED([TransitionID] ASC),

//    CONSTRAINT[FK_BaoGia_WorkflowTransition_Workflow]
//        FOREIGN KEY([WorkflowID]) REFERENCES[dbo].[BaoGia_Workflow] ([WorkflowID]),
//    CONSTRAINT[FK_BaoGia_WorkflowTransition_FromStep]
//        FOREIGN KEY([FromWorkflowStepID]) REFERENCES[dbo].[BaoGia_WorkflowStep] ([WorkflowStepID]),
//    CONSTRAINT[FK_BaoGia_WorkflowTransition_ToStep]
//        FOREIGN KEY([ToWorkflowStepID]) REFERENCES[dbo].[BaoGia_WorkflowStep] ([WorkflowStepID])
//);

//CREATE UNIQUE INDEX[UQ_BaoGia_WorkflowTransition_Source_Action]
//    ON[dbo].[BaoGia_WorkflowTransition] ([WorkflowID] ASC, [FromWorkflowStepID] ASC, [ActionCode] ASC);
