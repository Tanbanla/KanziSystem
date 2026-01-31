using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class REQUEST_DETAIL_VENDOR
{
    public int Id_Vendor { get; set; }

    public string? Code_Request { get; set; }

    public string? Vendor { get; set; }

    public string? VendorCode { get; set; }

    public string? VendorName { get; set; }

    public string? PriceUnit { get; set; }

    public string? WhoInPut { get; set; }

    public DateTime? TimeInput { get; set; }

    public string? Position { get; set; }

    public bool? Pass { get; set; }
}
