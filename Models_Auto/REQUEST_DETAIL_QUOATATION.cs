using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class REQUEST_DETAIL_QUOATATION
{
    public int Id_Quotation { get; set; }

    public int? Dong { get; set; }

    public int? Cot { get; set; }

    public string? Vendor { get; set; }

    public string? GoodCode { get; set; }

    public string? Code_Request { get; set; }

    public DateTime? UploadTime { get; set; }

    public string? Pic { get; set; }

    public string? FileName { get; set; }

    public string? FileLink { get; set; }

    public string? URL { get; set; }

    public string? Note { get; set; }

    public string? Kind { get; set; }

    public bool? SaveAs { get; set; }

    public string? Position { get; set; }

    public bool? Pass { get; set; }
}
