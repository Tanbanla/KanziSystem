using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class REQUEST
{
    public int Id_Request { get; set; }

    public string Code_Request { get; set; } = null!;

    public string? Cost_Center { get; set; }

    public DateOnly Request_Date { get; set; }

    public string? Declaration { get; set; }

    public DateOnly? Dealine { get; set; }

    public DateOnly? Dealine_Real { get; set; }

    public double? Total_exchange { get; set; }

    public double? Exchange_rate { get; set; }

    public string? Currency { get; set; }

    public double? Total { get; set; }

    public double? Total_exchange_real { get; set; }

    public double? Exchange_rate_Real { get; set; }

    public string? Currency_Real { get; set; }

    public double? Total_Real { get; set; }

    public string? Kind { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public DateTime? Create_Date { get; set; }

    public string? User_Create { get; set; }

    public DateTime? Last_Update { get; set; }

    public string? User_Update { get; set; }

    public string? Reason { get; set; }

    public string? Action { get; set; }

    public string? Place { get; set; }

    public bool? Freeze { get; set; }

    public string? Note { get; set; }

    public string? Loaihinhtokhai { get; set; }

    public string? Phuongthucvanchuyen { get; set; }

    public string? Group_Code { get; set; }

    public bool? Chophepin { get; set; }

    public string? KindofRQ { get; set; }

    public bool? Urgent { get; set; }

    public string? CostCenter { get; set; }

    public virtual DEPARTMENT? Cost_CenterNavigation { get; set; }

    public virtual ICollection<WF_PROCESS> WF_PROCESSes { get; set; } = new List<WF_PROCESS>();
}
