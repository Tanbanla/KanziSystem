using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class IM_PO_AUTO
{
    public int Id_PO { get; set; }

    public string SoPO { get; set; } = null!;

    public string? Loaichiphi { get; set; }

    public DateOnly? Ngayphathanh { get; set; }

    public string? Maphongban { get; set; }

    public string? Tenphongban { get; set; }

    public string? Bophan { get; set; }

    public string? Mucdich { get; set; }

    public string? MaNCC { get; set; }

    public string? TenNCC { get; set; }

    public string? KhuvucNCC { get; set; }

    public string? DiachiNCC { get; set; }

    public string? SodienthoaiNCC { get; set; }

    public string? SofaxNCC { get; set; }

    public DateTime? Ngaytao { get; set; }

    public string? Nguoilamdon { get; set; }

    public string? Hinhthuc { get; set; }

    public string? Group_Code { get; set; }

    public string? Danhmuc { get; set; }

    public string? TinhtrangPO { get; set; }

    public string? TinhtranghaiquanPO { get; set; }

    public string? TinhtranghaiquanPONguoinhap { get; set; }

    public DateTime? TinhtranghaiquanPONgaynhap { get; set; }

    public string? Loaitien { get; set; }

    public string? Lydo { get; set; }

    public string? Nguoixacnhan { get; set; }

    public DateTime? Thoigianxacnhan { get; set; }

    public bool? NNCNEW { get; set; }
}
