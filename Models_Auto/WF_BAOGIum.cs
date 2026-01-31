using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class WF_BAOGIum
{
    public int Id_Baogia { get; set; }

    public string? MaRequest { get; set; }

    public string? MaHangTem { get; set; }

    public string? MaHang { get; set; }

    public string? NCC { get; set; }

    public double? DonGia { get; set; }

    public double? DonGiaUSD { get; set; }

    public string? DonVi { get; set; }

    public string? FilePDF { get; set; }

    public string? Link { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? NguoiTao { get; set; }

    public bool? Choice { get; set; }

    public DateOnly? NgayLayBaoGia { get; set; }

    public string? GhiChu { get; set; }

    public string? Loai { get; set; }
}
