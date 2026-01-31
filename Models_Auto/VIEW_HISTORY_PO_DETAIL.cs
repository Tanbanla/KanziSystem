using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class VIEW_HISTORY_PO_DETAIL
{
    public string Mahang { get; set; } = null!;

    public string? SoPO { get; set; }

    public DateOnly? Issue_PO_Date { get; set; }

    public decimal? UnitPriceUSD { get; set; }

    public string? Dovi { get; set; }

    public double? Soluong { get; set; }

    public string? TenNCC { get; set; }
}
