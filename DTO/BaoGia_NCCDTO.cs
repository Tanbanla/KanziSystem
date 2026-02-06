using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class BaoGia_NCCDTO
{
    public int ID { get; set; }

    public string CHR_MaHang { get; set; } = null!;

    public string CHR_MaNCC { get; set; } = null!;

    public string NVCHAR_TenNCC { get; set; } = null!;

    public string CHR_CreateBy { get; set; } = null!;

    public DateTime DTM_CreateDate { get; set; }

    public string? CHR_UpdateBY { get; set; }

    public DateTime? DTM_UpdateDate { get; set; }

    public string? NVCHR_CodeByNCC { get; set; }

    public string? NVCHR_MakeIn { get; set; }
}
