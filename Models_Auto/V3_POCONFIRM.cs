using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class V3_POCONFIRM
{
    public int Id { get; set; }

    public string? Q1 { get; set; }

    public string? Q2 { get; set; }

    public string? Q3 { get; set; }

    public string? PO { get; set; }

    public string? UserConfirm { get; set; }

    public DateTime? DateConfirm { get; set; }
}
