using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class BaoGia_History_Approver_of_QuotationDTO
{
    public int ID { get; set; }

    public int ID_RequestQuote { get; set; }

    public int ID_BaoGiaStep { get; set; }

    public string CHR_UserSendApprover { get; set; } = null!;

    public DateTime DTM_UserSendApprover { get; set; }

    public string? CHR_UserApprover { get; set; }

    public DateTime? DTM_UserApprover { get; set; }

    public string? CHR_StatusFlag { get; set; }

    public bool? BIT_SendMail { get; set; }

    public string? NVCHR_ReturnReason { get; set; }

    public bool? BIT_Return { get; set; }

    public string? CHR_SectionCodeSend { get; set; }

    public string? CHR_SectionNameSend { get; set; }

    public string? CHR_SectionCodeApprover { get; set; }

    public string? CHR_SectionNameApprover { get; set; }
}
