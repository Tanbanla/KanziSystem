CREATE TABLE [dbo].[BaoGia_RequestType]
(
    [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CHR_Code] VARCHAR(50) NOT NULL,
    [NVCHR_Name] NVARCHAR(255) NOT NULL
);
CREATE TABLE [dbo].[BaoGia_RoleUser]
(
    [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserAdid] VARCHAR(30) NOT NULL,
    [Role] NVARCHAR(30) NOT NULL,
    [IsUsing] bit not null CONSTRAINT DF_BaoGia_RoleUser_IsUsing DEFAULT (1)
);
INSERT INTO [COST_MANAGEMENT].[dbo].[BaoGia_RoleUser]
(
	[UserAdid]
	,[Role]
)
SELECT
CHR_USERID,
CHR_CODE_MENU
FROM [COST_MANAGEMENT].[dbo].[TM_AUTHORITY_MENU]
 where CHR_CODE_MENU in ('UserPur','UserShip')
/* ============================================================
   1. Danh mục role
   ============================================================ */

IF OBJECT_ID(N'dbo.BaoGia_WorkflowRole', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BaoGia_WorkflowRole
    (
        RoleCode VARCHAR(20) NOT NULL,
        RoleName NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowRole_IsActive DEFAULT (1),

        CONSTRAINT PK_BaoGia_WorkflowRole PRIMARY KEY (RoleCode)
    );
END;
GO

INSERT INTO dbo.BaoGia_WorkflowRole
(
    RoleCode,
    RoleName
)
SELECT RoleCode, RoleName
FROM
(
    VALUES
        ('PUR',   N'Purchasing'),
        ('GA',    N'General Affairs'),
        ('SHIP',  N'Shipping'),
        ('USER',  N'Người yêu cầu'),
        ('ADMIN', N'Quản trị hệ thống')
) AS RoleData(RoleCode, RoleName)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.BaoGia_WorkflowRole AS ExistingRole
    WHERE ExistingRole.RoleCode = RoleData.RoleCode
);
GO


/* ============================================================
   2. Các bước lớn trong quy trình
   ============================================================ */

IF OBJECT_ID(N'dbo.BaoGia_WorkflowStage', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BaoGia_WorkflowStage
    (
        StageID INT IDENTITY(1, 1) NOT NULL,
        StageCode VARCHAR(50) NOT NULL,
        StageName NVARCHAR(200) NOT NULL,
        StageOrder INT NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowStage_IsActive DEFAULT (1),

        CONSTRAINT PK_BaoGia_WorkflowStage PRIMARY KEY (StageID),
        CONSTRAINT UQ_BaoGia_WorkflowStage_StageCode UNIQUE (StageCode),
        CONSTRAINT UQ_BaoGia_WorkflowStage_StageOrder UNIQUE (StageOrder)
    );
END;
GO

INSERT INTO dbo.BaoGia_WorkflowStage
(
    StageCode,
    StageName,
    StageOrder
)
SELECT StageCode, StageName, StageOrder
FROM
(
    VALUES
        ('SURVEY',        N'Xin khảo sát',              1),
        ('QUOTATION',     N'Yêu cầu báo giá',            2),
        ('CUSTOMS_NAME',  N'Xác nhận tên hải quan',      3)
) AS StageData(StageCode, StageName, StageOrder)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.BaoGia_WorkflowStage AS ExistingStage
    WHERE ExistingStage.StageCode = StageData.StageCode
);
GO


/* ============================================================
   3. Danh mục các bước nhỏ dùng chung
   ============================================================ */

IF OBJECT_ID(N'dbo.BaoGia_WorkflowStep', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BaoGia_WorkflowStep
    (
        StepID INT IDENTITY(1, 1) NOT NULL,
        StageID INT NOT NULL,
        StepCode VARCHAR(100) NOT NULL,
        StepName NVARCHAR(300) NOT NULL,
        StepNameEN NVARCHAR(300) NULL,
        Description NVARCHAR(1000) NULL,
        DefaultDurationHours INT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowStep_IsActive DEFAULT (1),

        CONSTRAINT PK_BaoGia_WorkflowStep PRIMARY KEY (StepID),
        CONSTRAINT UQ_BaoGia_WorkflowStep_StepCode UNIQUE (StepCode),
        CONSTRAINT FK_BaoGia_WorkflowStep_Stage
            FOREIGN KEY (StageID)
            REFERENCES dbo.BaoGia_WorkflowStage(StageID),
        CONSTRAINT CK_BaoGia_WorkflowStep_DefaultDurationHours
            CHECK (DefaultDurationHours IS NULL OR DefaultDurationHours >= 0)
    );
END;
GO

INSERT INTO dbo.BaoGia_WorkflowStep
(
    StageID,
    StepCode,
    StepName,
    StepNameEN,
    Description,
    DefaultDurationHours
)
SELECT
    Stage.StageID,
    StepData.StepCode,
    StepData.StepName,
    StepData.StepNameEN,
    StepData.Description,
    StepData.DefaultDurationHours
FROM
(
    VALUES
        ('SURVEY_RECEIVE',
            N'Tiếp nhận yêu cầu khảo sát',
            N'Receive survey request',
            N'Kiểm tra yêu cầu khảo sát ban đầu',
            8),

        ('SURVEY_CHECK_INFO',
            N'Kiểm tra thông tin khảo sát',
            N'Check survey information',
            N'Kiểm tra địa điểm, nội dung và thời gian khảo sát',
            8),

        ('SURVEY_ASSIGN',
            N'Phân công người khảo sát',
            N'Assign survey staff',
            N'Phân công người hoặc bộ phận thực hiện khảo sát',
            8),

        ('SURVEY_RESULT',
            N'Cập nhật kết quả khảo sát',
            N'Update survey result',
            N'Nhập biên bản, hình ảnh hoặc kết quả khảo sát',
            24),

        ('QUOTATION_CREATE',
            N'Tạo yêu cầu báo giá',
            N'Create quotation request',
            N'Tạo yêu cầu và danh sách mặt hàng cần báo giá',
            8),

        ('QUOTATION_CHECK',
            N'Kiểm tra thông tin báo giá',
            N'Check quotation information',
            N'Kiểm tra mã hàng, số lượng, đơn vị tính và nhà cung cấp',
            8),

        ('QUOTATION_SEND',
            N'Gửi yêu cầu báo giá nhà cung cấp',
            N'Send quotation request to suppliers',
            N'Gửi yêu cầu báo giá đến nhà cung cấp',
            8),

        ('QUOTATION_RECEIVE',
            N'Tiếp nhận báo giá',
            N'Receive supplier quotation',
            N'Nhập hoặc import báo giá từ nhà cung cấp',
            24),

        ('QUOTATION_COMPARE',
            N'So sánh báo giá',
            N'Compare quotations',
            N'So sánh giá, điều kiện giao hàng và điều kiện thanh toán',
            8),

        ('QUOTATION_APPROVE',
            N'Phê duyệt kết quả báo giá',
            N'Approve quotation result',
            N'Phê duyệt nhà cung cấp và kết quả lựa chọn',
            24),

        ('CUSTOMS_REQUEST',
            N'Gửi yêu cầu xác nhận tên hải quan',
            N'Request customs name confirmation',
            N'Gửi thông tin mặt hàng cần xác nhận tên hải quan',
            8),

        ('CUSTOMS_CHECK',
            N'Kiểm tra thông tin hải quan',
            N'Check customs information',
            N'Kiểm tra tên hàng, catalogue, thông số kỹ thuật và chứng từ',
            8),

        ('CUSTOMS_CONFIRM',
            N'Xác nhận tên hải quan',
            N'Confirm customs name',
            N'Xác nhận tên hàng và mã hàng sử dụng cho khai báo hải quan',
            24)
) AS StepData
(
    StepCode,
    StepName,
    StepNameEN,
    Description,
    DefaultDurationHours
)
INNER JOIN dbo.BaoGia_WorkflowStage AS Stage
    ON Stage.StageCode =
    CASE
        WHEN StepData.StepCode LIKE 'SURVEY_%' THEN 'SURVEY'
        WHEN StepData.StepCode LIKE 'QUOTATION_%' THEN 'QUOTATION'
        WHEN StepData.StepCode LIKE 'CUSTOMS_%' THEN 'CUSTOMS_NAME'
    END
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.BaoGia_WorkflowStep AS ExistingStep
    WHERE ExistingStep.StepCode = StepData.StepCode
);
GO


/* ============================================================
   4. Workflow theo loại hàng và luồng xử lý
      FlowCode: GA hoặc PUR
   ============================================================ */

IF OBJECT_ID(N'dbo.BaoGia_WorkflowDefinition', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BaoGia_WorkflowDefinition
    (
        WorkflowID INT IDENTITY(1, 1) NOT NULL,
        RequestTypeID INT NOT NULL,
        FlowCode VARCHAR(20) NOT NULL,
        WorkflowName NVARCHAR(300) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowDefinition_IsActive DEFAULT (1),
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_BaoGia_WorkflowDefinition_CreatedDate DEFAULT (SYSDATETIME()),
        CreatedBy NVARCHAR(100) NULL,
        UpdatedDate DATETIME2(0) NULL,
        UpdatedBy NVARCHAR(100) NULL,

        CONSTRAINT PK_BaoGia_WorkflowDefinition PRIMARY KEY (WorkflowID),
        CONSTRAINT UQ_BaoGia_WorkflowDefinition_Type_Flow
            UNIQUE (RequestTypeID, FlowCode),
        CONSTRAINT CK_BaoGia_WorkflowDefinition_FlowCode
            CHECK (FlowCode IN ('GA', 'PUR')),

        CONSTRAINT FK_BaoGia_WorkflowDefinition_RequestType
            FOREIGN KEY (RequestTypeID)
            REFERENCES dbo.BaoGia_RequestType(ID)
    );
END;
GO


/* Bổ sung 4 loại hàng nếu chưa tồn tại */
INSERT INTO dbo.BaoGia_RequestType
(
    CHR_Code,
    NVCHR_Name
)
SELECT RequestTypeData.CHR_Code, RequestTypeData.NVCHR_Name
FROM
(
    VALUES
        ('GOODS',         N'Hàng hóa'),
        ('DESIGN_GOODS',  N'Hàng hóa thiết kế'),
        ('SERVICE',       N'Dịch vụ'),
        ('RENOVATION',    N'Cải tạo / công trình')
) AS RequestTypeData(CHR_Code, NVCHR_Name)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.BaoGia_RequestType AS ExistingRequestType
    WHERE ExistingRequestType.CHR_Code = RequestTypeData.CHR_Code
);
GO


/* Tạo 8 workflow mặc định: 4 loại hàng x 2 luồng GA/PUR */
INSERT INTO dbo.BaoGia_WorkflowDefinition
(
    RequestTypeID,
    FlowCode,
    WorkflowName,
    CreatedBy
)
SELECT
    RequestType.ID,
    FlowData.FlowCode,
    CONCAT(RequestType.NVCHR_Name, N' - Luồng ', FlowData.FlowCode),
    N'SYSTEM'
FROM dbo.BaoGia_RequestType AS RequestType
CROSS JOIN
(
    VALUES
        ('GA'),
        ('PUR')
) AS FlowData(FlowCode)
WHERE RequestType.CHR_Code IN
(
    'GOODS',
    'DESIGN_GOODS',
    'SERVICE',
    'RENOVATION'
)
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.BaoGia_WorkflowDefinition AS ExistingWorkflow
    WHERE ExistingWorkflow.RequestTypeID = RequestType.ID
      AND ExistingWorkflow.FlowCode = FlowData.FlowCode
);
GO


/* ============================================================
   5. Các bước được bật trong từng workflow
   ============================================================ */

IF OBJECT_ID(N'dbo.BaoGia_WorkflowDefinitionStep', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BaoGia_WorkflowDefinitionStep
    (
        WorkflowStepID INT IDENTITY(1, 1) NOT NULL,
        WorkflowID INT NOT NULL,
        StepID INT NOT NULL,
        StepOrder INT NOT NULL,
        IsEnabled BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowDefinitionStep_IsEnabled DEFAULT (1),
        IsRequired BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowDefinitionStep_IsRequired DEFAULT (1),
        AllowSkip BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowDefinitionStep_AllowSkip DEFAULT (0),
        IsFinalStep BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowDefinitionStep_IsFinalStep DEFAULT (0),
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_BaoGia_WorkflowDefinitionStep_CreatedDate DEFAULT (SYSDATETIME()),
        CreatedBy NVARCHAR(100) NULL,

        CONSTRAINT PK_BaoGia_WorkflowDefinitionStep PRIMARY KEY (WorkflowStepID),
        CONSTRAINT UQ_BaoGia_WorkflowDefinitionStep_Workflow_Step
            UNIQUE (WorkflowID, StepID),
        CONSTRAINT UQ_BaoGia_WorkflowDefinitionStep_Workflow_Order
            UNIQUE (WorkflowID, StepOrder),

        CONSTRAINT FK_BaoGia_WorkflowDefinitionStep_Workflow
            FOREIGN KEY (WorkflowID)
            REFERENCES dbo.BaoGia_WorkflowDefinition(WorkflowID),

        CONSTRAINT FK_BaoGia_WorkflowDefinitionStep_Step
            FOREIGN KEY (StepID)
            REFERENCES dbo.BaoGia_WorkflowStep(StepID),

        CONSTRAINT CK_BaoGia_WorkflowDefinitionStep_StepOrder
            CHECK (StepOrder > 0)
    );
END;
GO


/* Mặc định bật toàn bộ bước nhỏ theo thứ tự bước lớn */
INSERT INTO dbo.BaoGia_WorkflowDefinitionStep
(
    WorkflowID,
    StepID,
    StepOrder,
    IsEnabled,
    IsRequired,
    AllowSkip,
    IsFinalStep,
    CreatedBy
)
SELECT
    Workflow.WorkflowID,
    Step.StepID,
    ROW_NUMBER() OVER
    (
        PARTITION BY Workflow.WorkflowID
        ORDER BY Stage.StageOrder, Step.StepID
    ),
    1,
    1,
    0,
    CASE
        WHEN Step.StepCode = 'CUSTOMS_CONFIRM' THEN 1
        ELSE 0
    END,
    N'SYSTEM'
FROM dbo.BaoGia_WorkflowDefinition AS Workflow
INNER JOIN dbo.BaoGia_WorkflowStep AS Step
    ON 1 = 1
INNER JOIN dbo.BaoGia_WorkflowStage AS Stage
    ON Stage.StageID = Step.StageID
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.BaoGia_WorkflowDefinitionStep AS ExistingDefinitionStep
    WHERE ExistingDefinitionStep.WorkflowID = Workflow.WorkflowID
      AND ExistingDefinitionStep.StepID = Step.StepID
);
GO


/* ============================================================
   6. Phân quyền theo role tại từng bước
   ============================================================ */

IF OBJECT_ID(N'dbo.BaoGia_WorkflowStepRole', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BaoGia_WorkflowStepRole
    (
        WorkflowStepID INT NOT NULL,
        RoleCode VARCHAR(20) NOT NULL,
        CanView BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowStepRole_CanView DEFAULT (1),
        CanProcess BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowStepRole_CanProcess DEFAULT (0),
        CanApprove BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowStepRole_CanApprove DEFAULT (0),
        CanReject BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowStepRole_CanReject DEFAULT (0),

        CONSTRAINT PK_BaoGia_WorkflowStepRole
            PRIMARY KEY (WorkflowStepID, RoleCode),

        CONSTRAINT FK_BaoGia_WorkflowStepRole_WorkflowStep
            FOREIGN KEY (WorkflowStepID)
            REFERENCES dbo.BaoGia_WorkflowDefinitionStep(WorkflowStepID),

        CONSTRAINT FK_BaoGia_WorkflowStepRole_Role
            FOREIGN KEY (RoleCode)
            REFERENCES dbo.BaoGia_WorkflowRole(RoleCode)
    );
END;
GO


/* ADMIN có toàn quyền trên tất cả bước */
INSERT INTO dbo.BaoGia_WorkflowStepRole
(
    WorkflowStepID,
    RoleCode,
    CanView,
    CanProcess,
    CanApprove,
    CanReject
)
SELECT
    DefinitionStep.WorkflowStepID,
    'ADMIN',
    1,
    1,
    1,
    1
FROM dbo.BaoGia_WorkflowDefinitionStep AS DefinitionStep
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.BaoGia_WorkflowStepRole AS ExistingPermission
    WHERE ExistingPermission.WorkflowStepID = DefinitionStep.WorkflowStepID
      AND ExistingPermission.RoleCode = 'ADMIN'
);
GO


/* USER: được xem và xử lý các bước khảo sát ban đầu */
INSERT INTO dbo.BaoGia_WorkflowStepRole
(
    WorkflowStepID,
    RoleCode,
    CanView,
    CanProcess,
    CanApprove,
    CanReject
)
SELECT
    DefinitionStep.WorkflowStepID,
    'USER',
    1,
    1,
    0,
    0
FROM dbo.BaoGia_WorkflowDefinitionStep AS DefinitionStep
INNER JOIN dbo.BaoGia_WorkflowStep AS Step
    ON Step.StepID = DefinitionStep.StepID
WHERE Step.StepCode IN
(
    'SURVEY_RECEIVE',
    'SURVEY_CHECK_INFO'
)
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.BaoGia_WorkflowStepRole AS ExistingPermission
    WHERE ExistingPermission.WorkflowStepID = DefinitionStep.WorkflowStepID
      AND ExistingPermission.RoleCode = 'USER'
);
GO


/* PUR: xử lý quy trình báo giá */
INSERT INTO dbo.BaoGia_WorkflowStepRole
(
    WorkflowStepID,
    RoleCode,
    CanView,
    CanProcess,
    CanApprove,
    CanReject
)
SELECT
    DefinitionStep.WorkflowStepID,
    'PUR',
    1,
    1,
    CASE WHEN Step.StepCode = 'QUOTATION_APPROVE' THEN 1 ELSE 0 END,
    CASE WHEN Step.StepCode IN ('QUOTATION_APPROVE', 'QUOTATION_CHECK') THEN 1 ELSE 0 END
FROM dbo.BaoGia_WorkflowDefinitionStep AS DefinitionStep
INNER JOIN dbo.BaoGia_WorkflowStep AS Step
    ON Step.StepID = DefinitionStep.StepID
WHERE Step.StepCode LIKE 'QUOTATION_%'
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.BaoGia_WorkflowStepRole AS ExistingPermission
    WHERE ExistingPermission.WorkflowStepID = DefinitionStep.WorkflowStepID
      AND ExistingPermission.RoleCode = 'PUR'
);
GO


/* GA: xử lý các bước liên quan đến GA */
INSERT INTO dbo.BaoGia_WorkflowStepRole
(
    WorkflowStepID,
    RoleCode,
    CanView,
    CanProcess,
    CanApprove,
    CanReject
)
SELECT
    DefinitionStep.WorkflowStepID,
    'GA',
    1,
    1,
    CASE WHEN Step.StepCode = 'QUOTATION_APPROVE' THEN 1 ELSE 0 END,
    CASE WHEN Step.StepCode = 'QUOTATION_APPROVE' THEN 1 ELSE 0 END
FROM dbo.BaoGia_WorkflowDefinitionStep AS DefinitionStep
INNER JOIN dbo.BaoGia_WorkflowStep AS Step
    ON Step.StepID = DefinitionStep.StepID
WHERE Step.StepCode IN
(
    'SURVEY_ASSIGN',
    'SURVEY_RESULT',
    'QUOTATION_CHECK',
    'QUOTATION_APPROVE'
)
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.BaoGia_WorkflowStepRole AS ExistingPermission
    WHERE ExistingPermission.WorkflowStepID = DefinitionStep.WorkflowStepID
      AND ExistingPermission.RoleCode = 'GA'
);
GO


/* SHIP: xử lý phần tên hải quan */
INSERT INTO dbo.BaoGia_WorkflowStepRole
(
    WorkflowStepID,
    RoleCode,
    CanView,
    CanProcess,
    CanApprove,
    CanReject
)
SELECT
    DefinitionStep.WorkflowStepID,
    'SHIP',
    1,
    1,
    CASE WHEN Step.StepCode = 'CUSTOMS_CONFIRM' THEN 1 ELSE 0 END,
    CASE WHEN Step.StepCode IN ('CUSTOMS_CHECK', 'CUSTOMS_CONFIRM') THEN 1 ELSE 0 END
FROM dbo.BaoGia_WorkflowDefinitionStep AS DefinitionStep
INNER JOIN dbo.BaoGia_WorkflowStep AS Step
    ON Step.StepID = DefinitionStep.StepID
WHERE Step.StepCode LIKE 'CUSTOMS_%'
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.BaoGia_WorkflowStepRole AS ExistingPermission
    WHERE ExistingPermission.WorkflowStepID = DefinitionStep.WorkflowStepID
      AND ExistingPermission.RoleCode = 'SHIP'
);
GO


/* ============================================================
   7. Phân quyền riêng theo từng user
      UserADID có thể thay bằng khóa chính bảng user hiện tại
   ============================================================ */

IF OBJECT_ID(N'dbo.BaoGia_WorkflowStepUser', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BaoGia_WorkflowStepUser
    (
        WorkflowStepID INT NOT NULL,
        UserADID NVARCHAR(100) NOT NULL,
        CanView BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowStepUser_CanView DEFAULT (1),
        CanProcess BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowStepUser_CanProcess DEFAULT (0),
        CanApprove BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowStepUser_CanApprove DEFAULT (0),
        CanReject BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowStepUser_CanReject DEFAULT (0),
        IsActive BIT NOT NULL CONSTRAINT DF_BaoGia_WorkflowStepUser_IsActive DEFAULT (1),

        CONSTRAINT PK_BaoGia_WorkflowStepUser
            PRIMARY KEY (WorkflowStepID, UserADID),

        CONSTRAINT FK_BaoGia_WorkflowStepUser_WorkflowStep
            FOREIGN KEY (WorkflowStepID)
            REFERENCES dbo.BaoGia_WorkflowDefinitionStep(WorkflowStepID)
    );
END;
GO


/* ============================================================
   8. Lịch sử thay đổi cấu hình workflow
   ============================================================ */

IF OBJECT_ID(N'dbo.BaoGia_WorkflowAudit', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BaoGia_WorkflowAudit
    (
        AuditID BIGINT IDENTITY(1, 1) NOT NULL,
        WorkflowID INT NULL,
        WorkflowStepID INT NULL,
        ActionCode VARCHAR(50) NOT NULL,
        OldValue NVARCHAR(MAX) NULL,
        NewValue NVARCHAR(MAX) NULL,
        ChangedBy NVARCHAR(100) NOT NULL,
        ChangedDate DATETIME2(0) NOT NULL CONSTRAINT DF_BaoGia_WorkflowAudit_ChangedDate DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_BaoGia_WorkflowAudit PRIMARY KEY (AuditID)
    );
END;
GO

-- lấy các bước thực hiện của đơn 
SELECT
    Workflow.WorkflowID,
    RequestType.CHR_Code AS RequestTypeCode,
    RequestType.NVCHR_Name AS RequestTypeName,
    Workflow.FlowCode,
    Workflow.WorkflowName,
    Stage.StageCode,
    Stage.StageName,
    Stage.StageOrder,
    Step.StepID,
    Step.StepCode,
    Step.StepName,
    DefinitionStep.WorkflowStepID,
    DefinitionStep.StepOrder,
    DefinitionStep.IsEnabled,
    DefinitionStep.IsRequired,
    DefinitionStep.AllowSkip,
    DefinitionStep.IsFinalStep
FROM dbo.BaoGia_WorkflowDefinition AS Workflow
INNER JOIN dbo.BaoGia_RequestType AS RequestType
    ON RequestType.ID = Workflow.RequestTypeID
INNER JOIN dbo.BaoGia_WorkflowDefinitionStep AS DefinitionStep
    ON DefinitionStep.WorkflowID = Workflow.WorkflowID
INNER JOIN dbo.BaoGia_WorkflowStep AS Step
    ON Step.StepID = DefinitionStep.StepID
INNER JOIN dbo.BaoGia_WorkflowStage AS Stage
    ON Stage.StageID = Step.StageID
WHERE Workflow.IsActive = 1
  AND DefinitionStep.IsEnabled = 1
  AND RequestType.CHR_Code = 'GOODS'
  AND Workflow.FlowCode =  'GA'
ORDER BY
    Stage.StageOrder,
    DefinitionStep.StepOrder;
-- kiểm tra quyền của user 
SELECT
    DefinitionStep.WorkflowStepID,
    Step.StepCode,
    Step.StepName,
    COALESCE(UserPermission.CanView, RolePermission.CanView, 0) AS CanView,
    COALESCE(UserPermission.CanProcess, RolePermission.CanProcess, 0) AS CanProcess,
    COALESCE(UserPermission.CanApprove, RolePermission.CanApprove, 0) AS CanApprove,
    COALESCE(UserPermission.CanReject, RolePermission.CanReject, 0) AS CanReject
FROM dbo.BaoGia_WorkflowDefinitionStep AS DefinitionStep
INNER JOIN dbo.BaoGia_WorkflowDefinition AS Workflow
    ON Workflow.WorkflowID = DefinitionStep.WorkflowID
INNER JOIN dbo.BaoGia_WorkflowStep AS Step
    ON Step.StepID = DefinitionStep.StepID
LEFT JOIN dbo.BaoGia_WorkflowStepRole AS RolePermission
    ON RolePermission.WorkflowStepID = DefinitionStep.WorkflowStepID
   AND RolePermission.RoleCode = @RoleCode
LEFT JOIN dbo.BaoGia_WorkflowStepUser AS UserPermission
    ON UserPermission.WorkflowStepID = DefinitionStep.WorkflowStepID
   AND UserPermission.UserADID = @UserADID
   AND UserPermission.IsActive = 1
WHERE Workflow.RequestTypeID = @RequestTypeID
  AND Workflow.FlowCode = @FlowCode
  AND DefinitionStep.IsEnabled = 1;

  -- thêm trường vảo bảng 
--  ALTER TABLE dbo.[REQUEST]
--ADD
--    WorkflowID INT NULL,
--    CurrentWorkflowStepID INT NULL,
--    WorkflowStatus VARCHAR(30) NULL,
--    WorkflowStartedDate DATETIME2(0) NULL,
--    WorkflowCompletedDate DATETIME2(0) NULL;
--GO