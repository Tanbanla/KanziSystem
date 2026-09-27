/*
    Seed cấu hình và phân quyền workflow.
    Chạy sau khi đã tạo các bảng trong SQLCreateTableImprover.sql.
    Có thể chạy lại nhiều lần, không tạo dữ liệu trùng.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

/* Bảng transition được thêm cho màn hình cấu hình tuyến chuyển bước. */
IF OBJECT_ID(N'dbo.BaoGia_WorkflowTransition', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BaoGia_WorkflowTransition
    (
        TransitionID INT IDENTITY(1, 1) NOT NULL,
        WorkflowID INT NOT NULL,
        FromWorkflowStepID INT NOT NULL,
        ToWorkflowStepID INT NOT NULL,
        ActionCode VARCHAR(30) NOT NULL,
        ConditionExpression NVARCHAR(2000) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowTransition_IsActive DEFAULT (1),
        CreatedBy NVARCHAR(100) NULL,
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_BaoGia_WorkflowTransition_CreatedDate DEFAULT (SYSDATETIME()),
        UpdatedBy NVARCHAR(100) NULL,
        UpdatedDate DATETIME2(0) NULL,
        CONSTRAINT PK_BaoGia_WorkflowTransition PRIMARY KEY (TransitionID),
        CONSTRAINT UQ_BaoGia_WorkflowTransition_Source_Action UNIQUE (WorkflowID, FromWorkflowStepID, ActionCode),
        CONSTRAINT FK_BaoGia_WorkflowTransition_Workflow FOREIGN KEY (WorkflowID) REFERENCES dbo.BaoGia_WorkflowDefinition(WorkflowID),
        CONSTRAINT FK_BaoGia_WorkflowTransition_FromStep FOREIGN KEY (FromWorkflowStepID) REFERENCES dbo.BaoGia_WorkflowDefinitionStep(WorkflowStepID),
        CONSTRAINT FK_BaoGia_WorkflowTransition_ToStep FOREIGN KEY (ToWorkflowStepID) REFERENCES dbo.BaoGia_WorkflowDefinitionStep(WorkflowStepID)
    );
END;

/* 1. Danh mục role dùng trong workflow */
INSERT INTO dbo.BaoGia_WorkflowRole (RoleCode, RoleName, IsActive)
SELECT RoleCode, RoleName, 1
FROM (VALUES
    ('ADMIN', 'Quản trị hệ thống'),
    ('USER',  'Người yêu cầu'),
    ('PUR',   'Purchasing'),
    ('GA',    'General Affairs'),
    ('SHIP',  'Shipping')
) AS Roles(RoleCode, RoleName)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.BaoGia_WorkflowRole AS ExistingRole
    WHERE ExistingRole.RoleCode = Roles.RoleCode
);

/* 2. Đảm bảo các workflow mặc định tồn tại cho 4 loại yêu cầu */
INSERT INTO dbo.BaoGia_RequestType (CHR_Code, NVCHR_Name)
SELECT RequestTypeCode, RequestTypeName
FROM (VALUES
    ('GOODS',        N'Hàng hóa'),
    ('DESIGN_GOODS', N'Hàng hóa thiết kế'),
    ('SERVICE',      N'Dịch vụ'),
    ('RENOVATION',   N'Cải tạo / công trình')
) AS RequestTypes(RequestTypeCode, RequestTypeName)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.BaoGia_RequestType AS ExistingType
    WHERE ExistingType.CHR_Code = RequestTypes.RequestTypeCode
);

INSERT INTO dbo.BaoGia_WorkflowDefinition
    (RequestTypeID, FlowCode, WorkflowName, IsActive, CreatedBy)
SELECT RequestType.ID, FlowCode, CONCAT(RequestType.NVCHR_Name, N' - Luồng ', FlowCode), 1, N'SYSTEM'
FROM dbo.BaoGia_RequestType AS RequestType
CROSS JOIN (VALUES ('GA'), ('PUR')) AS Flows(FlowCode)
WHERE RequestType.CHR_Code IN ('GOODS', 'DESIGN_GOODS', 'SERVICE', 'RENOVATION')
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.BaoGia_WorkflowDefinition AS ExistingWorkflow
      WHERE ExistingWorkflow.RequestTypeID = RequestType.ID
        AND ExistingWorkflow.FlowCode = Flows.FlowCode
  );

/* 3. Gắn toàn bộ step catalog đang hoạt động vào từng workflow.
      Nếu chỉ muốn một số step, thêm điều kiện StepCode bên dưới. */
INSERT INTO dbo.BaoGia_WorkflowDefinitionStep
    (WorkflowID, StepID, StepOrder, IsEnabled, IsRequired, AllowSkip, IsFinalStep, CreatedBy)
SELECT
    WorkflowDefinition.WorkflowID,
    WorkflowStep.StepID,
    ROW_NUMBER() OVER
    (
        PARTITION BY WorkflowDefinition.WorkflowID
        ORDER BY WorkflowStage.StageOrder, WorkflowStep.StepOrder, WorkflowStep.StepID
    ),
    1,
    1,
    0,
    CASE WHEN WorkflowStep.StepCode = 'CUSTOMS_CONFIRM' THEN 1 ELSE 0 END,
    N'SYSTEM'
FROM dbo.BaoGia_WorkflowDefinition AS WorkflowDefinition
CROSS JOIN dbo.BaoGia_WorkflowStep AS WorkflowStep
INNER JOIN dbo.BaoGia_WorkflowStage AS WorkflowStage
    ON WorkflowStage.StageID = WorkflowStep.StageID
WHERE WorkflowDefinition.IsActive = 1
  AND WorkflowStep.IsActive = 1
  AND WorkflowStage.IsActive = 1
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.BaoGia_WorkflowDefinitionStep AS ExistingStep
      WHERE ExistingStep.WorkflowID = WorkflowDefinition.WorkflowID
        AND ExistingStep.StepID = WorkflowStep.StepID
  );

/* 4. Phân quyền theo role tại từng step.
      ADMIN: toàn quyền.
      USER: xử lý các bước khảo sát ban đầu.
      PUR: xử lý và duyệt quy trình báo giá.
      GA: xử lý các bước khảo sát và kiểm tra/phê duyệt báo giá.
      SHIP: xử lý và duyệt các bước hải quan. */
INSERT INTO dbo.BaoGia_WorkflowStepRole
    (WorkflowStepID, RoleCode, CanView, CanProcess, CanApprove, CanReject)
SELECT DefinitionStep.WorkflowStepID, Permission.RoleCode,
       Permission.CanView, Permission.CanProcess, Permission.CanApprove, Permission.CanReject
FROM dbo.BaoGia_WorkflowDefinitionStep AS DefinitionStep
INNER JOIN dbo.BaoGia_WorkflowStep AS WorkflowStep
    ON WorkflowStep.StepID = DefinitionStep.StepID
CROSS APPLY
(
    SELECT 'ADMIN' AS RoleCode, CAST(1 AS bit) AS CanView, CAST(1 AS bit) AS CanProcess, CAST(1 AS bit) AS CanApprove, CAST(1 AS bit) AS CanReject
    UNION ALL
    SELECT 'USER', 1, CASE WHEN WorkflowStep.StepCode IN ('SURVEY_RECEIVE', 'SURVEY_CHECK_INFO') THEN 1 ELSE 0 END, 0, 0
    UNION ALL
    SELECT 'PUR', 1, CASE WHEN WorkflowStep.StepCode LIKE 'QUOTATION_%' THEN 1 ELSE 0 END, CASE WHEN WorkflowStep.StepCode = 'QUOTATION_APPROVE' THEN 1 ELSE 0 END, CASE WHEN WorkflowStep.StepCode LIKE 'QUOTATION_%' THEN 1 ELSE 0 END
    UNION ALL
    SELECT 'GA', 1, CASE WHEN WorkflowStep.StepCode IN ('SURVEY_ASSIGN', 'SURVEY_RESULT', 'QUOTATION_CHECK', 'QUOTATION_APPROVE') THEN 1 ELSE 0 END, CASE WHEN WorkflowStep.StepCode = 'QUOTATION_APPROVE' THEN 1 ELSE 0 END, CASE WHEN WorkflowStep.StepCode IN ('QUOTATION_CHECK', 'QUOTATION_APPROVE') THEN 1 ELSE 0 END
    UNION ALL
    SELECT 'SHIP', 1, CASE WHEN WorkflowStep.StepCode LIKE 'CUSTOMS_%' THEN 1 ELSE 0 END, CASE WHEN WorkflowStep.StepCode LIKE 'CUSTOMS_%' THEN 1 ELSE 0 END, CASE WHEN WorkflowStep.StepCode IN ('CUSTOMS_CHECK', 'CUSTOMS_CONFIRM') THEN 1 ELSE 0 END
) AS Permission
WHERE
(
    Permission.CanView = 1
    OR Permission.CanProcess = 1
    OR Permission.CanApprove = 1
    OR Permission.CanReject = 1
)
AND
(
    NOT EXISTS
    (
        SELECT 1
        FROM dbo.BaoGia_WorkflowStepRole AS ExistingPermission
        WHERE ExistingPermission.WorkflowStepID = DefinitionStep.WorkflowStepID
          AND ExistingPermission.RoleCode = Permission.RoleCode
    )
);

/* 5. Quyền trực tiếp theo user.
      Thay các ADID mẫu bên dưới bằng ADID thật trước khi chạy.
      Nếu chưa cần quyền trực tiếp theo user, có thể bỏ qua block này. */
DECLARE @UserPermissions TABLE
(
    UserADID NVARCHAR(100) NOT NULL,
    StepCode VARCHAR(100) NOT NULL,
    CanView BIT NOT NULL,
    CanProcess BIT NOT NULL,
    CanApprove BIT NOT NULL,
    CanReject BIT NOT NULL
);

INSERT INTO @UserPermissions
    (UserADID, StepCode, CanView, CanProcess, CanApprove, CanReject)
VALUES
    (N'REPLACE_WITH_PUR_ADID', 'QUOTATION_APPROVE', 1, 1, 1, 1),
    (N'REPLACE_WITH_GA_ADID',  'QUOTATION_CHECK',   1, 1, 1, 1);

INSERT INTO dbo.BaoGia_WorkflowStepUser
    (WorkflowStepID, UserADID, CanView, CanProcess, CanApprove, CanReject, IsActive)
SELECT DefinitionStep.WorkflowStepID, UserPermission.UserADID,
       UserPermission.CanView, UserPermission.CanProcess,
       UserPermission.CanApprove, UserPermission.CanReject, 1
FROM @UserPermissions AS UserPermission
INNER JOIN dbo.BaoGia_WorkflowStep AS WorkflowStep
    ON WorkflowStep.StepCode = UserPermission.StepCode
INNER JOIN dbo.BaoGia_WorkflowDefinitionStep AS DefinitionStep
    ON DefinitionStep.StepID = WorkflowStep.StepID
WHERE UserPermission.UserADID NOT LIKE N'REPLACE_WITH_%'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.BaoGia_WorkflowStepUser AS ExistingPermission
      WHERE ExistingPermission.WorkflowStepID = DefinitionStep.WorkflowStepID
        AND ExistingPermission.UserADID = UserPermission.UserADID
  );

/* 6. Transition tuyến tính mẫu cho từng workflow.
      Có thể chỉnh lại các transition sau khi chạy bằng màn hình WorkflowTransitions. */
IF OBJECT_ID(N'dbo.BaoGia_WorkflowTransition', N'U') IS NOT NULL
BEGIN
    /* Bảng có thể đã được tạo từ phiên bản cũ thiếu các cột này. */
    IF COL_LENGTH(N'dbo.BaoGia_WorkflowTransition', N'ConditionExpression') IS NULL
        ALTER TABLE dbo.BaoGia_WorkflowTransition ADD ConditionExpression NVARCHAR(2000) NULL;
    IF COL_LENGTH(N'dbo.BaoGia_WorkflowTransition', N'IsActive') IS NULL
        ALTER TABLE dbo.BaoGia_WorkflowTransition ADD IsActive BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowTransition_IsActive_Seed DEFAULT (1);
    IF COL_LENGTH(N'dbo.BaoGia_WorkflowTransition', N'CreatedBy') IS NULL
        ALTER TABLE dbo.BaoGia_WorkflowTransition ADD CreatedBy NVARCHAR(100) NULL;

    /* Dùng dynamic SQL để SQL Server biên dịch INSERT sau khi ALTER TABLE hoàn tất. */
    EXEC sys.sp_executesql N'
        INSERT INTO dbo.BaoGia_WorkflowTransition
        (
            WorkflowID, FromWorkflowStepID, ToWorkflowStepID,
            ActionCode, ConditionExpression, IsActive, CreatedBy
        )
        SELECT FromStep.WorkflowID,
               FromStep.WorkflowStepID,
               ToStep.WorkflowStepID,
               ''APPROVE'', NULL, 1, N''SYSTEM''
        FROM dbo.BaoGia_WorkflowDefinitionStep AS FromStep
        INNER JOIN dbo.BaoGia_WorkflowDefinitionStep AS ToStep
            ON ToStep.WorkflowID = FromStep.WorkflowID
           AND ToStep.StepOrder = FromStep.StepOrder + 1
        WHERE FromStep.IsEnabled = 1
          AND ToStep.IsEnabled = 1
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.BaoGia_WorkflowTransition AS ExistingTransition
              WHERE ExistingTransition.WorkflowID = FromStep.WorkflowID
                AND ExistingTransition.FromWorkflowStepID = FromStep.WorkflowStepID
                AND ExistingTransition.ActionCode = ''APPROVE''
          );';
END;

COMMIT TRANSACTION;

/* Kiểm tra kết quả */
SELECT 'WorkflowDefinition' AS TableName, COUNT(*) AS RowCount FROM dbo.BaoGia_WorkflowDefinition
UNION ALL
SELECT 'WorkflowDefinitionStep', COUNT(*) FROM dbo.BaoGia_WorkflowDefinitionStep
UNION ALL
SELECT 'WorkflowStepRole', COUNT(*) FROM dbo.BaoGia_WorkflowStepRole
UNION ALL
SELECT 'WorkflowStepUser', COUNT(*) FROM dbo.BaoGia_WorkflowStepUser;

SELECT
    WorkflowDefinition.FlowCode,
    RequestType.CHR_Code AS RequestTypeCode,
    WorkflowStep.StepCode,
    RolePermission.RoleCode,
    RolePermission.CanView,
    RolePermission.CanProcess,
    RolePermission.CanApprove,
    RolePermission.CanReject
FROM dbo.BaoGia_WorkflowStepRole AS RolePermission
INNER JOIN dbo.BaoGia_WorkflowDefinitionStep AS DefinitionStep
    ON DefinitionStep.WorkflowStepID = RolePermission.WorkflowStepID
INNER JOIN dbo.BaoGia_WorkflowDefinition AS WorkflowDefinition
    ON WorkflowDefinition.WorkflowID = DefinitionStep.WorkflowID
INNER JOIN dbo.BaoGia_RequestType AS RequestType
    ON RequestType.ID = WorkflowDefinition.RequestTypeID
INNER JOIN dbo.BaoGia_WorkflowStep AS WorkflowStep
    ON WorkflowStep.StepID = DefinitionStep.StepID
ORDER BY RequestType.CHR_Code, WorkflowDefinition.FlowCode, DefinitionStep.StepOrder, RolePermission.RoleCode;
