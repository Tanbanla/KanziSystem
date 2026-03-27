using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class IM_NCC_NEWDTO
{
    public int Ncc_Id { get; set; }

    public string Ma { get; set; } = null!;

    public string? Ten { get; set; }

    public string? Diachi { get; set; }

    public string? Sodienthoai { get; set; }

    public string? Fax { get; set; }

    public string? Khuvuc { get; set; }

    public string? Ghichu { get; set; }

    public string? Hinhthucmotk { get; set; }

    public string? Dieukienthanhtoan { get; set; }

    public string? Masothue { get; set; }

    public string? Nhanvienkinhdoand { get; set; }

    public string? Nhanvienketoan { get; set; }

    public string? Canphaixacnhanlamthutuchaiquan { get; set; }

    public bool? Xoa { get; set; }

    public string? nhom { get; set; }

    public string? nguoi_cap_nhat { get; set; }
    public string? ShortName { get; set; }
}
