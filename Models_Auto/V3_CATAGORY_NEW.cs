using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class V3_CATAGORY_NEW
{
    public int Id_Catagory { get; set; }

    public string? CategoryCode { get; set; }

    public string? Category1EN { get; set; }

    public string? Category2EN { get; set; }

    public string? Category3EN { get; set; }

    public string? Category1VN { get; set; }

    public string? Category2VN { get; set; }

    public string? Category3VN { get; set; }

    public bool? DeleteFlag { get; set; }
}
