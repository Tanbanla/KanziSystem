using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_NCC_Category
{
    public int ID { get; set; }

    public string CHR_MaNCC { get; set; } = null!;

    public string NVCHR_TenNCC { get; set; } = null!;

    public string? NVCHR_ChungLoai { get; set; }

    public string? NVCHR_SanXuat { get; set; }

    public string? CHR_Status { get; set; }

    public string CHR_CreateBy { get; set; } = null!;

    public DateTime? DTM_CreateBy { get; set; }

    public string? CHR_PIC { get; set; }

    public string? CHR_Mail { get; set; }
}
