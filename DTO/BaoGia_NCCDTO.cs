using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class BaoGia_NCCDTO
{
    public int ID { get; set; }

    public string CHR_MaHang { get; set; } = null!;

    public string CHR_MaNCC { get; set; } = null!;

    public string NVCHAR_TenNCC { get; set; } = null!;
}
